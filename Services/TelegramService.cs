using Telegram.Bot;
using Telegram.Bot.Types;

namespace RVMSService.Services
{
    public class TelegramService : ITelegramService
    {
        private readonly TelegramBotClient _botClient;
        private readonly ILogger<TelegramService> _logger;

        public TelegramService(IConfiguration configuration, ILogger<TelegramService> logger)
        {
            _logger = logger;
            var token = configuration["TelegramBotToken"];
            if (string.IsNullOrWhiteSpace(token))
            {
                _logger.LogWarning("TelegramBotToken is not configured in settings.");
            }
            _botClient = new TelegramBotClient(token ?? string.Empty);
        }

        public async Task SendVisitNotificationAsync(string chatId, string visitorName, string destinationAddress, List<byte[]> photos)
        {
            try
            {
                var message = $"🔔 *New Visitor Sign-In*\n" +
                              $"👤 Visitor: {EscapeMarkdown(visitorName)}\n" +
                              $"📍 Destination: {EscapeMarkdown(destinationAddress)}\n" +
                              $"🕐 Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

                foreach (var photo in photos)
                {
                    using var stream = new MemoryStream(photo);
                    await _botClient.SendPhoto(
                        chatId: chatId,
                        photo: InputFile.FromStream(stream, "visitor_photo.jpg"));
                }

                _logger.LogInformation("Telegram notification sent to chat {ChatId} for visitor {Visitor}", chatId, visitorName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Telegram notification to chat {ChatId}", chatId);
            }
        }

        private static string EscapeMarkdown(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            foreach (var c in new[] { "_", "*", "[", "]", "(", ")", "~", "`", ">", "#", "+", "-", "=", "|", "{", "}", ".", "!" })
            {
                text = text.Replace(c, "\\" + c);
            }
            return text;
        }

        public async Task<string> GetBotLinkAsync(Guid destinationId)
        {
            try
            {
                var me = await _botClient.GetMe();
                var link = $"https://t.me/{me.Username}?start={destinationId}";
                _logger.LogInformation("Generated Telegram deep link for destination {DestinationId}: {Link}", destinationId, link);
                return link;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate Telegram bot link");
                throw;
            }
        }
    }
}
