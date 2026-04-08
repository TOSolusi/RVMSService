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

        //public async Task SendVisitNotificationAsync(string chatId, string visitorName, string destinationAddress, List<byte[]> photos)
        //{
        //    try
        //    {
        //        var message = $"🔔 *New Visitor Sign-In*\n" +
        //                      $"👤 Visitor: {EscapeMarkdown(visitorName)}\n" +
        //                      $"📍 Destination: {EscapeMarkdown(destinationAddress)}\n" +
        //                      $"🕐 Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";

        //        await _botClient.SendMessage(
        //            chatId: chatId,
        //            text: message,
        //            parseMode: Telegram.Bot.Types.Enums.ParseMode.Markdown);

        //        foreach (var photo in photos)
        //        {
        //            using var stream = new MemoryStream(photo);
        //            await _botClient.SendPhoto(
        //                chatId: chatId,
        //                photo: InputFile.FromStream(stream, "visitor_photo.jpg"));
        //        }

        //        _logger.LogInformation("Telegram notification sent to chat {ChatId} for visitor {Visitor}", chatId, visitorName);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Failed to send Telegram notification to chat {ChatId}", chatId);
        //    }
        //}


        public async Task SendVisitNotificationAsync(string chatId, string visitorName, string destinationAddress, List<byte[]> photos, List<string> titles)
        {
            try
            {
                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Settings", "TelegramMessageTemplate.html"); // This works in development but may fail in production due to different working directory
                var templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Settings", "TelegramMessageTemplate.html");
                string message;

                if (!System.IO.File.Exists(templatePath))
                {
                    _logger.LogWarning("Telegram template not found at {Path}. Using default message.", templatePath);

                    message = $"🔔 <b>New Visitor Sign-In</b>\n" +
                              $"👤 Visitor: {EscapeHtml(visitorName)}\n" +
                              $"📍 Destination: {EscapeHtml(destinationAddress)}\n" +
                              $"🕐 Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                }
                else
                {
                    var templateContent = await System.IO.File.ReadAllTextAsync(templatePath);
                    message = templateContent
                        .Replace("{{Visitor}}", EscapeHtml(visitorName))
                        .Replace("{{Destination}}", EscapeHtml(destinationAddress))
                        .Replace("{{Time}}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                }

                await _botClient.SendMessage(
                    chatId: chatId,
                    text: message,
                    parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);

                for (int i = 0; i < photos.Count; i++)
                {
                    using var stream = new MemoryStream(photos[i]);
                    await _botClient.SendPhoto(
                        chatId: chatId,
                        photo: InputFile.FromStream(stream, "visitor_photo.jpg"),
                        caption: i < titles.Count ? EscapeHtml(titles[i]) : null,
                        parseMode: Telegram.Bot.Types.Enums.ParseMode.Html);
                }

                //foreach (var photo in photos)
                //{
                //    using var stream = new MemoryStream(photo);
                //    await _botClient.SendPhoto(
                //        chatId: chatId,
                //        photo: InputFile.FromStream(stream, "visitor_photo.jpg"));
                //}

                _logger.LogInformation("Telegram notification sent to chat {ChatId} for visitor {Visitor}", chatId, visitorName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Telegram notification to chat {ChatId}", chatId);
            }
        }

        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            // Telegram HTML requires escaping <, >, and & to prevent user input from breaking the HTML structure
            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;");
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
