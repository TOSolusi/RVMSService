using Microsoft.EntityFrameworkCore;
using RVMSService.Data;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace RVMSService.Services
{
    public class TelegramBotPollingService : BackgroundService
    {
        private readonly ILogger<TelegramBotPollingService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly TelegramBotClient _botClient;

        public TelegramBotPollingService(
            IConfiguration configuration,
            ILogger<TelegramBotPollingService> logger,
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;

            var token = configuration["TelegramBotToken"];
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("TelegramBotToken is not configured. Telegram polling will not work.");
            }
            _botClient = new TelegramBotClient(token ?? string.Empty);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Telegram bot polling service started.");

            int offset = 0;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var updates = await _botClient.GetUpdates(
                        offset: offset,
                        limit: 100,
                        timeout: 30,
                        allowedUpdates: new[] { UpdateType.Message },
                        cancellationToken: stoppingToken);

                    foreach (var update in updates)
                    {
                        offset = update.Id + 1;

                        if (update.Message?.Text == null)
                            continue;

                        await HandleMessageAsync(update.Message, stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error polling Telegram updates. Retrying in 5 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }

            _logger.LogInformation("Telegram bot polling service stopped.");
        }

        private async Task HandleMessageAsync(Message message, CancellationToken ct)
        {
            var text = message.Text!.Trim();
            var chatId = message.Chat.Id.ToString();
            var chatName = message.Chat.FirstName ?? message.Chat.Username ?? "Unknown";

            _logger.LogInformation("Received Telegram message from {ChatName} ({ChatId}): {Text}", chatName, chatId, text);

            // Handle /start <destinationId>
            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length < 2 || !Guid.TryParse(parts[1], out var destinationId))
                {
                    await _botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: "👋 Welcome! To link your account, use the link provided by your administrator.",
                        cancellationToken: ct);
                    return;
                }

                await LinkChatIdToDestination(destinationId, chatId, chatName, message.Chat.Id, ct);
                return;
            }

            // Default reply for any other message
            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "ℹ️ I only respond to /start commands from deep links. Contact your administrator for help.",
                cancellationToken: ct);
        }

        private async Task LinkChatIdToDestination(Guid destinationId, string chatId, string chatName, long telegramChatId, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDBContext>();

            var destination = await dbContext.Destinations.FindAsync(new object[] { destinationId }, ct);

            if (destination == null)
            {
                _logger.LogWarning("Destination {DestinationId} not found for Telegram link request from {ChatId}", destinationId, chatId);
                await _botClient.SendMessage(
                    chatId: telegramChatId,
                    text: "❌ Destination not found. Please check the link with your administrator.",
                    cancellationToken: ct);
                return;
            }

            destination.Owner_TelegramChatId = chatId;
            await dbContext.SaveChangesAsync(ct);

            _logger.LogInformation("Linked Telegram chat {ChatId} ({ChatName}) to destination {DestinationId} ({Address})",
                chatId, chatName, destinationId, destination.Address);

            await _botClient.SendMessage(
                chatId: telegramChatId,
                text: $"✅ Success! You are now linked to destination:\n📍 *{destination.Address}*\n\nYou will receive notifications when visitors sign in.",
                parseMode: ParseMode.Markdown,
                cancellationToken: ct);
        }
    }
}
