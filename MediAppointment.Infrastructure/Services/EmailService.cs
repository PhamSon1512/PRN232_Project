using System.Net;
using System.Net.Mail;
using MediAppointment.Application.Interfaces;
using MediAppointment.Infrastructure.Persistence.Configurations;
using Microsoft.Extensions.Options;

namespace MediAppointment.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailConfig _config;

        public EmailService(IOptions<EmailConfig> config)
        {
            _config = config.Value;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort)
            {
                Credentials = new NetworkCredential(_config.SmtpUser, _config.SmtpPass),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_config.SenderEmail, _config.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(to);

            await client.SendMailAsync(mail);
        }
    }
}
