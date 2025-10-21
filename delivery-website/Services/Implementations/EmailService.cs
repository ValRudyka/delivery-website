using delivery_website.Models.Configuration;
using delivery_website.Services.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace delivery_website.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
                message.To.Add(MailboxAddress.Parse(toEmail));
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = htmlBody
                };
                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();

                await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Email sent successfully to {toEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email to {toEmail}: {ex.Message}");
                throw;
            }
        }

        public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, string userName)
        {
            var subject = "Скидання паролю - Платформа доставки їжі";
            var htmlBody = GetPasswordResetEmailTemplate(resetLink, userName);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendEmailVerificationAsync(string toEmail, string verificationLink, string userName)
        {
            var subject = "Підтвердження email - Платформа доставки їжі";
            var htmlBody = GetEmailVerificationTemplate(verificationLink, userName);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendWelcomeEmailAsync(string toEmail, string userName)
        {
            var subject = "Ласкаво просимо! - Платформа доставки їжі";
            var htmlBody = GetWelcomeEmailTemplate(userName);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        public async Task SendOrderConfirmationEmailAsync(string toEmail, string orderNumber, decimal totalAmount)
        {
            var subject = $"Підтвердження замовлення #{orderNumber}";
            var htmlBody = GetOrderConfirmationTemplate(orderNumber, totalAmount);
            await SendEmailAsync(toEmail, subject, htmlBody);
        }

        private string GetPasswordResetEmailTemplate(string resetLink, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #999; font-size: 12px; }}
        .warning {{ background: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Скидання паролю</h1>
        </div>
        <div class='content'>
            <h2>Привіт, {userName}!</h2>
            <p>Ми отримали запит на скидання паролю для вашого акаунту на платформі доставки їжі.</p>
            <p>Щоб створити новий пароль, натисніть на кнопку нижче:</p>
            <div style='text-align: center;'>
                <a href='{resetLink}' class='button'>Скинути пароль</a>
            </div>
            <p>Або скопіюйте це посилання у ваш браузер:</p>
            <p style='word-break: break-all; background: white; padding: 10px; border-radius: 5px;'>{resetLink}</p>
            <div class='warning'>
                <strong>⚠️ Важливо:</strong> Це посилання дійсне протягом 24 годин. Якщо ви не запитували скидання паролю, просто проігноруйте цей лист.
            </div>
            <p>З повагою,<br>Команда платформи доставки їжі</p>
        </div>
        <div class='footer'>
            <p>© 2025 Платформа доставки їжі. Усі права захищені.</p>
            <p>Якщо у вас виникли питання, зв'яжіться з нами: support@delivery.ua</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetEmailVerificationTemplate(string verificationLink, string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #28a745; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✉️ Підтвердження Email</h1>
        </div>
        <div class='content'>
            <h2>Привіт, {userName}!</h2>
            <p>Дякуємо за реєстрацію на нашій платформі!</p>
            <p>Будь ласка, підтвердіть вашу email адресу, натиснувши на кнопку нижче:</p>
            <div style='text-align: center;'>
                <a href='{verificationLink}' class='button'>Підтвердити Email</a>
            </div>
            <p>Або скопіюйте це посилання у ваш браузер:</p>
            <p style='word-break: break-all; background: white; padding: 10px; border-radius: 5px;'>{verificationLink}</p>
            <p>З повагою,<br>Команда платформи доставки їжі</p>
        </div>
        <div class='footer'>
            <p>© 2025 Платформа доставки їжі. Усі права захищені.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .button {{ display: inline-block; padding: 15px 30px; background: #667eea; color: white; text-decoration: none; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #999; font-size: 12px; }}
        .features {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎉 Ласкаво просимо!</h1>
        </div>
        <div class='content'>
            <h2>Привіт, {userName}!</h2>
            <p>Дякуємо, що приєдналися до нашої платформи доставки їжі!</p>
            <div class='features'>
                <h3>Що ви можете зробити:</h3>
                <ul>
                    <li>🍕 Замовляти їжу з 500+ ресторанів</li>
                    <li>🚀 Швидка доставка за 30-40 хвилин</li>
                    <li>⭐ Залишати відгуки та оцінки</li>
                    <li>💰 Отримувати знижки та бонуси</li>
                </ul>
            </div>
            <p style='text-align: center;'>
                <a href='https://localhost:7224/Customer/Restaurants' class='button'>Почати замовляти</a>
            </p>
            <p>З повагою,<br>Команда платформи доставки їжі</p>
        </div>
        <div class='footer'>
            <p>© 2025 Платформа доставки їжі. Усі права захищені.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetOrderConfirmationTemplate(string orderNumber, decimal totalAmount)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 30px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f9f9f9; padding: 30px; border-radius: 0 0 10px 10px; }}
        .order-box {{ background: white; padding: 20px; border-radius: 5px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 30px; color: #999; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Замовлення підтверджено!</h1>
        </div>
        <div class='content'>
            <h2>Дякуємо за ваше замовлення!</h2>
            <div class='order-box'>
                <h3>Деталі замовлення:</h3>
                <p><strong>Номер замовлення:</strong> {orderNumber}</p>
                <p><strong>Загальна сума:</strong> {totalAmount:F2} ₴</p>
            </div>
            <p>Ваше замовлення прийнято і готується. Ви отримаєте повідомлення про статус доставки найближчим часом.</p>
            <p>З повагою,<br>Команда платформи доставки їжі</p>
        </div>
        <div class='footer'>
            <p>© 2025 Платформа доставки їжі. Усі права захищені.</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}