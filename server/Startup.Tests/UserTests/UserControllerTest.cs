using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.UserTests;

[TestFixture]
public class UserControllerTest
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;
    private User _testUser = null!;

    [SetUp]
    public async Task Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig(); // uses your extension method
                });
            });

        _client = _factory.CreateClient();

        // Seed test user
        _testUser = MockObjects.GetUser();
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        db.Users.Add(_testUser);
        await db.SaveChangesAsync();

        // Login and set JWT
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { Email = _testUser.Email, Password = "pass" });
        loginResp.EnsureSuccessStatusCode();
        var authDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        _client.DefaultRequestHeaders.Add("Authorization", authDto!.Jwt);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnOk()
    {
        var response = await _client.DeleteAsync("api/User/DeleteUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PatchUserEmail_ShouldReturnOk()
    {
        var patchDto = new PatchUserEmailDto
        {
            OldEmail = _testUser.Email,
            NewEmail = "newemail@example.com"
        };

        var response = await _client.PatchAsJsonAsync("api/User/PatchUserEmail", patchDto);
        
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnOk()
    {
        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "pass",
            NewPassword = "newpass123"
        };

        var response = await _client.PatchAsJsonAsync("api/User/PatchUserPassword", patchDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "wrongpass",
            NewPassword = "newpass123"
        };

        var response = await _client.PatchAsJsonAsync("api/User/PatchUserPassword", patchDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        var unauthClient = _factory.CreateClient(); // No JWT
        var response = await unauthClient.DeleteAsync("api/User/DeleteUser");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
