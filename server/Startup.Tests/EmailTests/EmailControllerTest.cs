using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos.EmailList.Request;
using Application.Services;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EmailTests;

[TestFixture]
public class EmailControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private User _testUser = null!;

    [SetUp]
    public async Task Setup()
    {
        _testUser = MockObjects.GetUser();
        
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig();
                });
            });

        _client = _factory.CreateClient();
        
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        db.EmailList.Add(new EmailList { Email = _testUser.Email });
        await db.SaveChangesAsync();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task SubscribeToEmailList_ShouldReturnOk()
    {
        var dto = new AddEmailDto { Email = "subscribe_test@example.com" };
        var response = await _client.PostAsJsonAsync("api/email/subscribe", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnsubscribeFromEmailList_ShouldReturnOk()
    {
        var dto = new RemoveEmailDto { Email = _testUser.Email };
        var response = await _client.PostAsJsonAsync("api/email/unsubscribe", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnsubscribeFromEmailListViaToken_ShouldReturnOk()
    {
        // Get JWT token from backend (simulate manually)
        using var scope = _factory.Services.CreateScope();
        var jwtService = scope.ServiceProvider.GetRequiredService<JwtEmailTokenService>();
        var token = jwtService.GenerateUnsubscribeToken(_testUser.Email);

        var response = await _client.GetAsync($"/api/email/unsubscribe?token={token}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task UnsubscribeFromEmailListViaToken_ShouldReturnBadRequest_WhenTokenInvalid()
    {
        var invalidToken = "this.is.invalid";
        var response = await _client.GetAsync($"/api/email/unsubscribe?token={invalidToken}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task SubscribeToEmailList_ShouldNotDuplicateEmail()
    {
        var dto = new AddEmailDto { Email = _testUser.Email };
        var response = await _client.PostAsJsonAsync("api/email/subscribe", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
    
    [Test]
    public async Task UnsubscribeFromEmailList_ShouldReturnOk_WhenEmailDoesNotExist()
    {
        var dto = new RemoveEmailDto { Email = "nonexistent@example.com" };
        var response = await _client.PostAsJsonAsync("api/email/unsubscribe", dto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }
}
