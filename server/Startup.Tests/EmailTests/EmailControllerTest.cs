using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Models;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EmailTests;

[TestFixture]
public class EmailControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScopeFactory _scopeFactory;
    
    
    private User _testUser;
    private Mock<IEmailSender> _emailSenderMock;
    
    [OneTimeSetUp]
    public async Task SetupOneTime()
    {
        _emailSenderMock = new Mock<IEmailSender>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig(useTestContainer: false, useInMemory: true);
                    
                    services.Configure<AppOptions>(opts => opts.EnableEmailSending = false);
                    services.PostConfigure<AppOptions>(opts => opts.Seed = false);
                    
                    services.RemoveAll<IEmailSender>();
                    services.AddSingleton(_emailSenderMock.Object);
                });
            });

        _client = _factory.CreateClient();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [OneTimeTearDown]
    public void TearDownOneTime()
    {
        _client.Dispose();
        _factory.Dispose();
    }
    
    [SetUp]
    public async Task Setup()
    {
        
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        
        _testUser = MockObjects.GetUser();
        db.EmailList.Add(new EmailList { Email = _testUser.Email });
        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _emailSenderMock.Reset();
    }

    
    [Test]
    public async Task SendEmail_ShouldInvokeEmailSenderAndReturnSuccessMessage()
    {
        // Arrange
        var request = new EmailRequest
        {
            Subject = "Test Subject",
            Message = "Hello world!"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"api/email/{EmailController.SendEmailRoute}", request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(content, Is.EqualTo("Email sent successfully."));
        });
        _emailSenderMock.Verify(
            es => es.SendEmailAsync("Test Subject", "Hello world!"),
            Times.Once
        );
    }
    
    [Test]
    public async Task SubscribeToEmailList_ShouldInvokeAddEmailAsync()
    {
        // Arrange
        var dto = new AddEmailDto { Email = "new@example.com" };

        // Act
        var response = await _client.PostAsJsonAsync($"api/email/{EmailController.SubscribeToEmailListRoute}", dto);

        // Assert
        _emailSenderMock.Verify(
            es => es.AddEmailAsync(It.Is<AddEmailDto>(d => d.Email == "new@example.com")),
            Times.Once
        );
        var body = await response.Content.ReadAsStringAsync();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.EqualTo("Subscription confirmed and email sent."));
        });
    }


    [Test]
    public async Task UnsubscribeFromEmailList_ShouldReturnOk()
    {
        var dto = new RemoveEmailDto { Email = _testUser.Email };
        var response = await _client.PostAsJsonAsync($"api/email/{EmailController.UnsubscribeFromEmailListRoute}", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo("Unsubscription confirmed and email sent."));
    }

    [Test]
    public async Task UnsubscribeFromEmailLink_ShouldReturnOkAndCorrectMessage_WhenTokenValid()
    {
        // Arrange
        using var scope = _scopeFactory.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtEmailTokenService>();
        var token = jwtService.GenerateUnsubscribeToken("user@example.com");

        // Act
        var response = await _client.GetAsync($"api/email/{EmailController.UnsubscribeFromEmailLinkRoute}?token={token}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(body, Is.EqualTo("You have been unsubscribed."));
        });
    }


    [Test]
    public async Task UnsubscribeFromEmailListViaToken_ShouldReturnBadRequest_WhenTokenInvalid()
    {
        const string invalidToken = "this.is.invalid";
        var response = await _client.GetAsync($"/api/email/{EmailController.UnsubscribeFromEmailLinkRoute}?token={invalidToken}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo("Invalid or expired token."));
    }

    [Test]
    public async Task SubscribeToEmailList_ShouldNotDuplicateEmail()
    {
        var dto = new AddEmailDto { Email = _testUser.Email };
        var response = await _client.PostAsJsonAsync($"api/email/{EmailController.SubscribeToEmailListRoute}", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnsubscribeFromEmailList_ShouldReturnOk_WhenEmailDoesNotExist()
    {
        var dto = new RemoveEmailDto { Email = "nonexistent@example.com" };
        var response = await _client.PostAsJsonAsync($"api/email/{EmailController.UnsubscribeFromEmailListRoute}", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}