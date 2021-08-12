using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TitanBlog.Services
{
    public class GmailSmtpService : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public GmailSmtpService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string emailTo, string subject, string htmlMessage)
        {
            //Step 1: Build the email itself...
            var email = new MimeMessage();
            email.Sender = MailboxAddress.Parse(_configuration["MailSettings:Email"]);
            email.To.Add(MailboxAddress.Parse(emailTo));
            email.Subject = subject;

            //Now for the body of the email
            var builder = new BodyBuilder();
            builder.HtmlBody = htmlMessage;

            email.Body = builder.ToMessageBody();

            //Step 2: Configure the smtp server
            using var smtp = new SmtpClient();
            smtp.Connect(_configuration["MailSettings:Host"],
                         Convert.ToInt32(_configuration["MailSettings:Port"]),
                         SecureSocketOptions.StartTls);

            smtp.Authenticate(_configuration["MailSettings:Email"], _configuration["MailSettings:Password"]);

            await smtp.SendAsync(email);

            smtp.Disconnect(true);
        }
    }
}