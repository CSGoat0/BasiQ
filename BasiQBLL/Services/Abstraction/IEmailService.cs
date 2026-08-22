namespace BasiQBLL.Services.Abstraction
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string email, string confirmationLink);
        Task SendPasswordResetAsync(string email, string resetLink);
        Task SendNotificationAsync(string email, string subject, string message);
        Task SendWelcomeEmailAsync(string email, string userName);
        Task SendPasswordChangedNotificationAsync(string email);
        Task SendCustomEmailAsync(string email, string subject, string body, bool isHtml = true);
    }
}
