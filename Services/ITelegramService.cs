namespace RVMSService.Services
{
    public interface ITelegramService
    {
        Task SendVisitNotificationAsync(string chatId, string visitorName, string destinationAddress, List<byte[]> photos, List<String> titles);
        Task<string> GetBotLinkAsync(Guid destinationId);
    }
}
