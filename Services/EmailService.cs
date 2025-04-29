using RideWild.Interfaces;
using RideWild.Models;
using System.Net;
using System.Net.Mail;

namespace RideWild.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        public async Task PswResetEmailAsync(string to, string subject, string emailContent)
        {
            var sender = _configuration["EmailDatas:From"];
            var client = _configuration["EmailDatas:Client"];
            var port = int.Parse(_configuration["EmailDatas:Port"]);
            var username = _configuration["EmailDatas:Username"];
            var password = _configuration["EmailDatas:Password"];

            var email = new MailMessage();
            email.From = new MailAddress(sender);
            email.To.Add(new MailAddress(to));
            email.Subject = subject;
            email.Body = emailContent;
            email.IsBodyHtml = true;

            using var smtp = new SmtpClient(client, port)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(email);
        }
    }
}