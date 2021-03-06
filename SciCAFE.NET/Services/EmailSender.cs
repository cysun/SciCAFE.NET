﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Security.Policy;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Scriban;

namespace SciCAFE.NET.Services
{
    public class EmailSettings
    {
        public string AppUrl { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public bool RequireAuthentication { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }

    public class EmailSender
    {
        private readonly EmailSettings _settings;
        private readonly string _templateFolder;

        private readonly UserService _userService;

        private ILogger<EmailSender> _logger;

        public EmailSender(IWebHostEnvironment env, IOptions<EmailSettings> settings,
            UserService userService, ILogger<EmailSender> logger)
        {
            _templateFolder = $"{env.ContentRootPath}/EmailTemplates";
            _settings = settings.Value;
            _userService = userService;
            _logger = logger;
        }

        public MimeMessage CreateEmailVerificationMessage(Models.User user, string link)
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

            _logger.LogInformation("Email verification message created for user {user}", user.UserName);

            return msg;
        }

        public MimeMessage CreateReviewEventMessage(Models.Event evnt)
        {
            var reviewers = _userService.GetEventReviewers();
            if (reviewers.Count == 0)
            {
                _logger.LogError("No event reviewers in the system");
                return null;
            }

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.AddRange(reviewers.Select(r => new MailboxAddress(r.Name, r.Email)).ToList());

            var template = Template.Parse(File.ReadAllText($"{_templateFolder}/ReviewEvent.Subject.txt"));
            msg.Subject = template.Render(new { evnt.Id });

            template = Template.Parse(File.ReadAllText($"{_templateFolder}/ReviewEvent.Body.txt"));
            msg.Body = new TextPart("html")
            {
                Text = template.Render(new { evnt, _settings.AppUrl })
            };

            _logger.LogInformation("ReviewEvent message created for event {event}", evnt.Id);

            return msg;
        }

        public MimeMessage CreateEventReviewedMessage(Models.Event evnt)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.Add(new MailboxAddress(evnt.Creator.Name, evnt.Creator.Email));

            var template = Template.Parse(File.ReadAllText($"{_templateFolder}/EventReviewed.Subject.txt"));
            msg.Subject = template.Render(new { evnt.Id });

            template = Template.Parse(File.ReadAllText($"{_templateFolder}/EventReviewed.Body.txt"));
            msg.Body = new TextPart("html")
            {
                Text = template.Render(new { evnt })
            };

            _logger.LogInformation("EventReviewed message created for event {event}", evnt.Id);

            return msg;
        }

        public MimeMessage CreateReviewRewardMessage(Models.Reward reward)
        {
            var reviewers = _userService.GetRewardReviewers();
            if (reviewers.Count == 0)
            {
                _logger.LogError("No reward reviewers in the system");
                return null;
            }

            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.AddRange(reviewers.Select(r => new MailboxAddress(r.Name, r.Email)).ToList());

            var template = Template.Parse(File.ReadAllText($"{_templateFolder}/ReviewReward.Subject.txt"));
            msg.Subject = template.Render(new { reward.Id });

            template = Template.Parse(File.ReadAllText($"{_templateFolder}/ReviewReward.Body.txt"));
            msg.Body = new TextPart("html")
            {
                Text = template.Render(new { reward, _settings.AppUrl })
            };

            _logger.LogInformation("ReviewReward message created for reward {reward}", reward.Id);

            return msg;
        }

        public MimeMessage CreateRewardReviewedMessage(Models.Reward reward)
        {
            var msg = new MimeMessage();
            msg.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            msg.To.Add(new MailboxAddress(reward.Creator.Name, reward.Creator.Email));

            var template = Template.Parse(File.ReadAllText($"{_templateFolder}/RewardReviewed.Subject.txt"));
            msg.Subject = template.Render(new { reward.Id });

            template = Template.Parse(File.ReadAllText($"{_templateFolder}/RewardReviewed.Body.txt"));
            msg.Body = new TextPart("html")
            {
                Text = template.Render(new { reward })
            };

            _logger.LogInformation("RewardReviewed message created for reward {reward}", reward.Id);

            return msg;
        }

        public MimeMessage CreateMessage(ClaimsPrincipal sender, List<Models.User> recipients,
            string subject, string content)
        {
            var msg = new MimeMessage();

            var senderName = $"{sender.FindFirstValue(ClaimTypes.GivenName)} {sender.FindFirstValue(ClaimTypes.Surname)}";
            var senderEmail = sender.FindFirstValue(ClaimTypes.Email);
            msg.From.Add(new MailboxAddress(senderName, senderEmail));
            msg.To.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
            foreach (Models.User user in recipients)
                msg.Bcc.Add(new MailboxAddress(user.Name, user.Email));

            msg.Subject = subject;
            msg.Body = new TextPart("html")
            {
                Text = content
            };

            return msg;
        }

        public async Task SendAsync(MimeMessage msg)
        {
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.None);
                if (_settings.RequireAuthentication)
                    await client.AuthenticateAsync(_settings.Username, _settings.Password);
                await client.SendAsync(msg);
                await client.DisconnectAsync(true);
            }
        }
    }
}
