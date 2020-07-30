using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using MimeKit;
using SciCAFE.NET.Models;
using Scriban;

namespace SciCAFE.NET.Services
{
    public class EmailSettings
    {
        public string AppUrl { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }

    public class EmailSender
    {
        private readonly EmailSettings _settings;
        private readonly string _templateFolder;

        public EmailSender(IWebHostEnvironment env, IOptions<EmailSettings> settings)
        {
            _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
            _settings = settings.Value;
        }

        public MimeMessage CreateEmailVerificationMessage(User user, string link)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.Add(new MailboxAddress(user.Name, user.Email));
            msg.Subject = "SciCAFE - Email Verification";

            var template = Template.Parse(File.ReadAllText($"{_templateFolder}/EmailVerification.Body.txt"));
            msg.Body = new TextPart("html")
            {
                Text = template.Render(new { link = $"{_settings.AppUrl}{link}" })
            };

            return msg;
        }

        public async Task SendAsync(MimeMessage msg)
        {
            using (var client = new SmtpClient())
            {
                await client.ConnectAsync(_settings.Host, _settings.Port, false);
                await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
        }
    }
}
