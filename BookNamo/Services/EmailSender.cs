// Service for sending emails using SMTP. Used for OTP, notifications, and other email communications.
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BookNamo.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailSender(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmail, _emailSettings.SenderName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                using (var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port))
                {
                    client.UseDefaultCredentials = false;
                    client.Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password);
                    client.EnableSsl = _emailSettings.EnableSsl;

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                // In a production environment, you would log this exception
                Console.WriteLine($"Email sending failed: {ex.Message}");
                throw;
            }
        }
    }

    public class EmailSettings
    {
        // Use required modifier to ensure these are initialized
        public required string Host { get; set; }
        public int Port { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string SenderEmail { get; set; }
        public required string SenderName { get; set; }
        public bool EnableSsl { get; set; }
    }
}
