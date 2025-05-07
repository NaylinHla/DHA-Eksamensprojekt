using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Core.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class EmailSenderService : IEmailSender
{
    private readonly IOptionsMonitor<AppOptions> _optionsMonitor;
    private readonly IEmailListRepository _emailListRepository;
    private readonly JwtEmailTokenService _jwtService;

    public EmailSenderService(
        IOptionsMonitor<AppOptions> optionsMonitor,
        IEmailListRepository emailListRepository,
        JwtEmailTokenService jwtService)
    {
        _optionsMonitor = optionsMonitor;
        _emailListRepository = emailListRepository;
        _jwtService = jwtService;
    }

    public async Task SendEmailAsync(string subject, string message)
    {
        var client = new SmtpClient("smtp.mailersend.net", 2525)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
                _optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD
            )
        };

        var emailList = _emailListRepository.GetAllEmails();

        foreach (var email in emailList)
        {
            string token = _jwtService.GenerateUnsubscribeToken(email);
            string unsubscribeUrl = $"https://meetyourplants.site/api/email/unsubscribe?token={token}";

            string htmlBody = $@"
                <p>{message}</p>
                <hr />
                <p style='font-size:12px;color:#888;'>
                    If you no longer wish to receive emails, click <a href='{unsubscribeUrl}'>unsubscribe</a>.
                </p>";

            var mailMessage = new MailMessage
            {
                From = new MailAddress("noreply@meetyourplants.site"),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            mailMessage.To.Add(email);
            await client.SendMailAsync(mailMessage);
        }
    }

    public async Task AddEmailAsync(AddEmailDto dto)
    {
        if (!_emailListRepository.EmailExists(dto.Email))
        {
            _emailListRepository.Add(new EmailList { Email = dto.Email });
            _emailListRepository.Save();

            await SendConfirmationEmailAsync(dto.Email);
        }
    }

    public async Task RemoveEmailAsync(RemoveEmailDto dto)
    {
        _emailListRepository.RemoveByEmail(dto.Email);
        _emailListRepository.Save();

        await SendGoodbyeEmailAsync(dto.Email);
    }

    private async Task SendConfirmationEmailAsync(string toEmail)
    {
        var client = new SmtpClient("smtp.mailersend.net", 2525)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
                _optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD
            )
        };

        var mailMessage = new MailMessage(
            from: "noreply@meetyourplants.site",
            to: toEmail,
            subject: "Welcome to Meet Your Plants!",
            body: "Thank you for subscribing to our email list. We’re excited to share updates with you!"
        );

        await client.SendMailAsync(mailMessage);
    }

    private async Task SendGoodbyeEmailAsync(string toEmail)
    {
        var client = new SmtpClient("smtp.mailersend.net", 2525)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(
                _optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
                _optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD
            )
        };

        var mailMessage = new MailMessage(
            from: "noreply@meetyourplants.site",
            to: toEmail,
            subject: "Goodbye from Meet Your Plants",
            body: "You've been unsubscribed from our email list. We're sad to see you go!"
        );

        await client.SendMailAsync(mailMessage);
    }
}
