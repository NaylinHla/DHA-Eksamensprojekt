using System.Net;
using System.Net.Mail;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Core.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Application.Services;

public interface ISmtpClient : IAsyncDisposable
{
    bool EnableSsl { get; set; }
    bool UseDefaultCredentials { get; set; }
    ICredentialsByHost Credentials { get; set; }
    Task SendMailAsync(MailMessage message);
}

public sealed class SmtpClientWrapper : ISmtpClient
{
    private readonly SmtpClient _inner;

    public SmtpClientWrapper(string host, int port) => _inner = new(host, port);

    public bool EnableSsl
    {
        get => _inner.EnableSsl;
        set => _inner.EnableSsl = value;
    }

    public bool UseDefaultCredentials
    {
        get => _inner.UseDefaultCredentials;
        set => _inner.UseDefaultCredentials = value;
    }

    public ICredentialsByHost Credentials
    {
        get => _inner.Credentials;
        set => _inner.Credentials = value;
    }

    public Task SendMailAsync(MailMessage message) => _inner.SendMailAsync(message);

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return ValueTask.CompletedTask;
    }
}

public delegate ISmtpClient SmtpClientFactory();


public class EmailSenderService(IOptionsMonitor<AppOptions> optionsMonitor,
    IEmailListRepository emailListRepository,
    JwtEmailTokenService jwtService,
    SmtpClientFactory clientFactory,
    IValidator<AddEmailDto> addEmailValidator,
    IValidator<RemoveEmailDto> removeEmailValidator) : IEmailSender
{

    public async Task SendEmailAsync(string subject, string message)
    {
        await using var client = clientFactory();
        client.EnableSsl            = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(
            optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
            optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD);

        foreach (var email in emailListRepository.GetAllEmails())
        {
            var token = jwtService.GenerateUnsubscribeToken(email);
            var unsubscribeUrl = $"https://meetyourplants.site/api/email/unsubscribe?token={token}";

            var htmlBody = $@"
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
        await addEmailValidator.ValidateAndThrowAsync(dto);
        if (!emailListRepository.EmailExists(dto.Email))
        {
            emailListRepository.Add(new EmailList { Email = dto.Email });
            emailListRepository.Save();

            await SendConfirmationEmailAsync(dto.Email);
        }
    }

    public async Task RemoveEmailAsync(RemoveEmailDto dto)
    {
        await removeEmailValidator.ValidateAndThrowAsync(dto);
        if (!emailListRepository.EmailExists(dto.Email))
            return;

        emailListRepository.RemoveByEmail(dto.Email);
        emailListRepository.Save();

        await SendGoodbyeEmailAsync(dto.Email);
    }

    private async Task SendConfirmationEmailAsync(string toEmail)
    {

        await using var client = clientFactory();
        client.EnableSsl            = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(
            optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
            optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD);

        var mailMessage = new MailMessage(
            "noreply@meetyourplants.site",
            toEmail,
            "Welcome to Meet Your Plants!",
            "Thank you for subscribing to our email list. We’re excited to share updates with you!"
        );

        await client.SendMailAsync(mailMessage);
    }


    private async Task SendGoodbyeEmailAsync(string toEmail)
    {
        await using var client = clientFactory();
        client.EnableSsl            = true;
        client.UseDefaultCredentials = false;
        client.Credentials = new NetworkCredential(
            optionsMonitor.CurrentValue.EMAIL_SENDER_USERNAME,
            optionsMonitor.CurrentValue.EMAIL_SENDER_PASSWORD);

        var mailMessage = new MailMessage(
            "noreply@meetyourplants.site",
            toEmail,
            "Goodbye from Meet Your Plants",
            "You've been unsubscribed from our email list. We're sad to see you go!"
        );

        await client.SendMailAsync(mailMessage);
    }
}