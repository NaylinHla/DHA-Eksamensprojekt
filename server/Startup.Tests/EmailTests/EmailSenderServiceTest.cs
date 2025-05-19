using System.Net;
using System.Net.Mail;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.EmailTests;

[TestFixture]
public class EmailSenderServiceTest
{
    private EmailSenderService _service;
    private Mock<IEmailListRepository> _emailRepo;
    private JwtEmailTokenService _jwtService;
    private Mock<ISmtpClient> _smtpMock;
    private SmtpClientFactory _factory;
    
    [SetUp]
    public void Setup()
    {
        _emailRepo = new Mock<IEmailListRepository>();

        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeUser",
            EMAIL_SENDER_PASSWORD = "fakePass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = false
        };

        var jwtOptionsMock = new Mock<IOptions<AppOptions>>();
        jwtOptionsMock.Setup(o => o.Value).Returns(appOptions);
        _jwtService = new JwtEmailTokenService(jwtOptionsMock.Object);

        var emailOptionsMonitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        emailOptionsMonitorMock.Setup(o => o.CurrentValue).Returns(appOptions);

        _smtpMock = new Mock<ISmtpClient>();
        _smtpMock.SetupProperty(c => c.EnableSsl);
        _smtpMock.SetupProperty(c => c.UseDefaultCredentials);
        _smtpMock.SetupProperty(c => c.Credentials);
        
        _factory = () => _smtpMock.Object;
        
        
        _service = new EmailSenderService(
            emailOptionsMonitorMock.Object, 
            _emailRepo.Object, 
            _jwtService,
            _factory,
            Mock.Of<IValidator<AddEmailDto>>(), 
            Mock.Of<IValidator<RemoveEmailDto>>());
    }

    

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
    public Task SendEmailAsync_ShouldNotSend_WhenDisabled()
    {
        _emailRepo.Setup(r => r.GetAllEmails()).Returns(["user1@example.com"]);

        Assert.DoesNotThrowAsync(() => _service.SendEmailAsync("Subject", "Body"));
        
        _smtpMock.Verify(c => c.SendMailAsync(It.IsAny<MailMessage>()), Times.Never);
        return Task.CompletedTask;
    }

    [Test]
    public async Task SendEmailAsync_ShouldGenerateUnsubscribeLinks_WhenEnabled()
    {
        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeUser",
            EMAIL_SENDER_PASSWORD = "fakePass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = true
        };

        var monitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        monitorMock.Setup(x => x.CurrentValue).Returns(appOptions);

        var jwtOpts = new Mock<IOptions<AppOptions>>();
        jwtOpts.Setup(x => x.Value).Returns(appOptions);
        var jwtSvc = new JwtEmailTokenService(jwtOpts.Object);
        
        
        var repo = new Mock<IEmailListRepository>();
        repo.Setup(r => r.GetAllEmails()).Returns([
            "a@example.com", "b@example.com"
        ]);

        var smtp = new Mock<ISmtpClient>();
        smtp.Setup(c => c.SendMailAsync(It.IsAny<MailMessage>()))
            .Returns(Task.CompletedTask);
        
        SmtpClientFactory factory = () => smtp.Object;
        
        var service = new EmailSenderService(
            monitorMock.Object, 
            repo.Object, 
            jwtSvc,
            factory,
            Mock.Of<IValidator<AddEmailDto>>(), 
            Mock.Of<IValidator<RemoveEmailDto>>());
        
        await service.SendEmailAsync("Test Subject", "Hello users!");
        

        repo.Verify(r => r.GetAllEmails(), Times.Once);
        smtp.Verify(c => c.SendMailAsync(It.IsAny<MailMessage>()), Times.Exactly(2));
    }

    [Test]
    public async Task RemoveEmailAsync_ShouldSendGoodbye_WhenEnabled()
    {
        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "fakeUser",
            EMAIL_SENDER_PASSWORD = "fakePass",
            JWT_EMAIL_SECRET = "test-secret-key-123456789012345678901234567890",
            EnableEmailSending = true
        };

        var monitorMock = new Mock<IOptionsMonitor<AppOptions>>();
        monitorMock.Setup(x => x.CurrentValue).Returns(appOptions);
        
        var jwtOptionsMock = new Mock<IOptions<AppOptions>>();
        jwtOptionsMock.Setup(x => x.Value).Returns(appOptions);
        
        var jwtService = new JwtEmailTokenService(jwtOptionsMock.Object);

        var repo = new Mock<IEmailListRepository>();
        repo.Setup(r => r.EmailExists(It.IsAny<string>())).Returns(true);

        var smtp = new Mock<ISmtpClient>();
        smtp.Setup(c => c.SendMailAsync(It.IsAny<MailMessage>())).Returns(Task.CompletedTask);
        
        SmtpClientFactory factory = () => smtp.Object;
        
        var service = new EmailSenderService(
            monitorMock.Object, 
            repo.Object, 
            jwtService, 
            factory,
            Mock.Of<IValidator<AddEmailDto>>(), 
            Mock.Of<IValidator<RemoveEmailDto>>());
        
        await service.RemoveEmailAsync(new RemoveEmailDto { Email = "bye@example.com" });
            
        repo.Verify(r => r.RemoveByEmail("bye@example.com"), Times.Once);
        repo.Verify(r => r.Save(), Times.Once);
        smtp.Verify(c => c.SendMailAsync(It.IsAny<MailMessage>()), Times.Once);
    }
    
    [Test]
    public async Task SendEmailAsync_ShouldSendHtmlMail_WithExpectedProperties()
    {
        /* ── separate arrange with emails + enabled sending ─────────────── */
        var appOptions = new AppOptions
        {
            EMAIL_SENDER_USERNAME = "user",
            EMAIL_SENDER_PASSWORD = "pwd",
            JWT_EMAIL_SECRET      = "test‑secret‑key‑123456789012345678901234567890",
            EnableEmailSending    = true
        };

        var optsMonitor = new Mock<IOptionsMonitor<AppOptions>>();
        optsMonitor.Setup(o => o.CurrentValue).Returns(appOptions);

        var jwtOpts = new Mock<IOptions<AppOptions>>();
        jwtOpts.Setup(o => o.Value).Returns(appOptions);
        var jwtSvc = new JwtEmailTokenService(jwtOpts.Object);

        var repo = new Mock<IEmailListRepository>();
        var emails = new List<string> { "alice@test.com", "bob@test.com" };
        repo.Setup(r => r.GetAllEmails()).Returns(emails);

        var smtp = new Mock<ISmtpClient>();
        smtp.SetupProperty(c => c.EnableSsl);
        smtp.SetupProperty(c => c.UseDefaultCredentials);
        smtp.SetupProperty(c => c.Credentials);

        var captured = new List<MailMessage>();
        smtp.Setup(c => c.SendMailAsync(It.IsAny<MailMessage>()))
            .Callback<MailMessage>(captured.Add)
            .Returns(Task.CompletedTask);

        SmtpClientFactory factory = () => smtp.Object;
        var service = new EmailSenderService(
            optsMonitor.Object, 
            repo.Object, 
            jwtSvc, 
            factory,
            Mock.Of<IValidator<AddEmailDto>>(), 
            Mock.Of<IValidator<RemoveEmailDto>>());

        // Act
        await service.SendEmailAsync("Weekly update", "Hello plant lovers!");

        // Assert
        smtp.VerifySet(c => c.EnableSsl             = true);
        smtp.VerifySet(c => c.UseDefaultCredentials = false);
        smtp.VerifySet(c => c.Credentials           = It.IsAny<NetworkCredential>());

        smtp.Verify(c => c.SendMailAsync(It.IsAny<MailMessage>()),
                    Times.Exactly(emails.Count));

        foreach (var m in captured)
        {
            Assert.Multiple(() =>
            {
                Assert.That(m.Body, Is.Not.Null.Or.Empty);
                Assert.That(m.Body, Does.Contain("Hello plant lovers!"));
                Assert.That(m.Body, Does.Contain("unsubscribe"));
            });
            Assert.Multiple(() =>
            {
                Assert.That(m.Subject, Is.Not.Null.Or.Empty);
                Assert.That(m.Subject, Is.EqualTo("Weekly update"));
                
            });
            Assert.Multiple(() =>
            {
                Assert.That(m.IsBodyHtml, Is.True);
                Assert.That(m.To, Has.Count.EqualTo(1));
            });
            
        }
    }
}