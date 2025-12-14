using System.Threading.Tasks;

namespace delivery_website.Services.Interfaces
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}