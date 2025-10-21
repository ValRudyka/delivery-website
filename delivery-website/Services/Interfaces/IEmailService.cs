namespace delivery_website.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlBody);
        Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName);
        Task SendEmailVerificationAsync(string toEmail, string verificationLink, string userName);
        Task SendWelcomeEmailAsync(string toEmail, string userName);
        Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber, decimal totalAmount);
    }
}