

using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace LitraLand.Web.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSettings _mailSettings;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailSender(IOptions<MailSettings> mailSettings, IWebHostEnvironment webHostEnvironment)
        {
            _mailSettings = mailSettings.Value;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MailMessage
            {
                From = new MailAddress(_mailSettings.Email!, _mailSettings.DisplayName),
                Body = htmlMessage,
                Subject = subject,
                IsBodyHtml = true
            };

            var isDevelopment = _webHostEnvironment.IsDevelopment();

            message.To.Add(isDevelopment ? "12422023663286@pg.cu.edu.eg" : email);

            var smtpClient = new SmtpClient(_mailSettings.Host, _mailSettings.Port)
            {
                Credentials = new NetworkCredential(_mailSettings.Email, _mailSettings.Password),
                EnableSsl = true
            };

            await smtpClient.SendMailAsync(message);

            smtpClient.Dispose();
        }
    }
}
