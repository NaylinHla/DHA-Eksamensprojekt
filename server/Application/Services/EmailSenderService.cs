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

    public EmailSenderService(
        IOptionsMonitor<AppOptions> optionsMonitor,
        IEmailListRepository emailListRepository)
    {
        _optionsMonitor = optionsMonitor;
        _emailListRepository = emailListRepository;
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
            var mailMessage = new MailMessage(
                from: "noreply@meetyourplants.site",
                to: email,
                subject,
                message
            );

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
