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

    public Task AddEmailAsync(AddEmailDto dto)
    {
        if (!_emailListRepository.EmailExists(dto.Email))
        {
            _emailListRepository.Add(new EmailList { Email = dto.Email });
            _emailListRepository.Save();
        }

        return Task.CompletedTask; // No confirmation email sent
    }

    public Task RemoveEmailAsync(RemoveEmailDto dto)
    {
        _emailListRepository.RemoveByEmail(dto.Email);
        _emailListRepository.Save();

        return Task.CompletedTask; // No goodbye email sent
    }
}
