namespace RVMSService.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task<string> LoadTemplateAsync(string templateFileName, Dictionary<string, string> placeholders);
    }
}
