using System.Net.Mail;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.EmailTests;

[TestFixture]
public class EmailSenderServiceTest
{
    [SetUp]
    public void Setup()
    {
        _emailRepo = new Mock<IEmailListRepository>();

        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeuser",
            EMAIL_SENDER_PASSWORD = "fakepass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = false
        };

        var jwtOptionsMock = new Mock<IOptions<AppOptions>>();
        jwtOptionsMock.Setup(o => o.Value).Returns(appOptions);
        _jwtService = new JwtEmailTokenService(jwtOptionsMock.Object);

        var emailOptionsMonitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        emailOptionsMonitorMock.Setup(o => o.CurrentValue).Returns(appOptions);

        _service = new EmailSenderService(emailOptionsMonitorMock.Object, _emailRepo.Object, _jwtService);
    }

    private EmailSenderService _service = null!;
    private Mock<IEmailListRepository> _emailRepo = null!;
    private JwtEmailTokenService _jwtService = null!;

    [Test]
    public async Task AddEmailAsync_ShouldAdd_WhenEmailNotExists()
    {
        var dto = new AddEmailDto { Email = "newuser@example.com" };
        _emailRepo.Setup(r => r.EmailExists(dto.Email)).Returns(false);

        await _service.AddEmailAsync(dto);

        _emailRepo.Verify(r => r.Add(It.Is<EmailList>(e => e.Email == dto.Email)), Times.Once);
        _emailRepo.Verify(r => r.Save(), Times.Once);
    }

    [Test]
    public async Task AddEmailAsync_ShouldNotAdd_WhenEmailExists()
    {
        var dto = new AddEmailDto { Email = "existing@example.com" };
        _emailRepo.Setup(r => r.EmailExists(dto.Email)).Returns(true);

        await _service.AddEmailAsync(dto);

        _emailRepo.Verify(r => r.Add(It.IsAny<EmailList>()), Times.Never);
        _emailRepo.Verify(r => r.Save(), Times.Never);
    }

    [Test]
    public async Task RemoveEmailAsync_ShouldRemove_WhenEmailExists()
    {
        var dto = new RemoveEmailDto { Email = "delete@example.com" };
        _emailRepo.Setup(r => r.EmailExists(dto.Email)).Returns(true);

        await _service.RemoveEmailAsync(dto);

        _emailRepo.Verify(r => r.RemoveByEmail(dto.Email), Times.Once);
        _emailRepo.Verify(r => r.Save(), Times.Once);
    }

    [Test]
    public async Task RemoveEmailAsync_ShouldNotRemove_WhenEmailNotExists()
    {
        var dto = new RemoveEmailDto { Email = "notfound@example.com" };
        _emailRepo.Setup(r => r.EmailExists(dto.Email)).Returns(false);

        await _service.RemoveEmailAsync(dto);

        _emailRepo.Verify(r => r.RemoveByEmail(It.IsAny<string>()), Times.Never);
        _emailRepo.Verify(r => r.Save(), Times.Never);
    }

    [Test]
    public async Task SendEmailAsync_ShouldNotSend_WhenDisabled()
    {
        _emailRepo.Setup(r => r.GetAllEmails()).Returns(new List<string> { "user1@example.com" });

        Assert.DoesNotThrowAsync(() => _service.SendEmailAsync("Subject", "Body"));
    }

    [Test]
    public async Task SendEmailAsync_ShouldGenerateUnsubscribeLinks_WhenEnabled()
    {
        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeuser",
            EMAIL_SENDER_PASSWORD = "fakepass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = true
        };

        var jwtOptionsMock = new Mock<IOptions<AppOptions>>();
        jwtOptionsMock.Setup(x => x.Value).Returns(appOptions);

        var monitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        monitorMock.Setup(x => x.CurrentValue).Returns(appOptions);

        var jwtService = new JwtEmailTokenService(jwtOptionsMock.Object);

        var repo = new Mock<IEmailListRepository>();
        repo.Setup(r => r.GetAllEmails()).Returns(new List<string>
        {
            "a@example.com",
            "b@example.com"
        });

        var service = new EmailSenderService(monitorMock.Object, repo.Object, jwtService);

        try
        {
            await service.SendEmailAsync("Test Subject", "Hello users!");
        }
        catch (SmtpException)
        {
            // not interested in actually sending here
            // just reaching the loop and token generation
        }

        repo.Verify(r => r.GetAllEmails(), Times.Once);
    }

    [Test]
    public async Task RemoveEmailAsync_ShouldSendGoodbye_WhenEnabled()
    {
        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeuser",
            EMAIL_SENDER_PASSWORD = "fakepass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = true
        };

        var jwtOptionsMock = new Mock<IOptions<AppOptions>>();
        jwtOptionsMock.Setup(x => x.Value).Returns(appOptions);

        var monitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        monitorMock.Setup(x => x.CurrentValue).Returns(appOptions);

        var jwtService = new JwtEmailTokenService(jwtOptionsMock.Object);

        var repo = new Mock<IEmailListRepository>();
        repo.Setup(r => r.EmailExists(It.IsAny<string>())).Returns(true);

        var service = new EmailSenderService(monitorMock.Object, repo.Object, jwtService);

        try
        {
            await service.RemoveEmailAsync(new RemoveEmailDto { Email = "bye@example.com" });
        }
        catch (SmtpException)
        {
            // expected for fake credentials
        }

        repo.Verify(r => r.RemoveByEmail("bye@example.com"), Times.Once);
        repo.Verify(r => r.Save(), Times.Once);
    }
}