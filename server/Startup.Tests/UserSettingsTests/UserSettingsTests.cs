using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserSettings.Response;
using Application.Models.Dtos.UserSettings;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.UserSettingsTests;

[TestFixture]
public class UserSettingsControllerTests : WebApplicationFactory<Program>
{
    private HttpClient _client;
    private User _testUser;
    private string _jwt;

    [SetUp]
    public async Task Setup()
    {
        _client = CreateClient();

        _testUser = MockObjects.GetUser();
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        db.Users.Add(_testUser);
        await db.SaveChangesAsync();

        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { _testUser.Email, Password = "Secret25!" });
        loginResp.EnsureSuccessStatusCode();
        var dto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(dto, Is.Not.Null);
        _jwt = dto.Jwt;
        _client.DefaultRequestHeaders.Add("Authorization", _jwt);
    }

    [Test]
    public async Task GetAllSettings_ReturnsCorrectValues()
    {
        var resp = await _client.GetAsync("/api/userSettings");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var settings = await resp.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(settings, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(settings.Celsius, Is.True);
            Assert.That(settings.DarkTheme, Is.False);
            Assert.That(settings.ConfirmDialog, Is.False);
            Assert.That(settings.SecretMode, Is.False);
        });
    }

    [Test]
    public async Task PatchSetting_UpdatesCorrectField_ConfirmDialog()
    {
        var patch = new UpdateUserSettingDto { Value = false };

        var resp = await _client.PatchAsJsonAsync("/api/userSettings/confirmDialog", patch);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var fetch = await _client.GetAsync("/api/userSettings");
        var settings = await fetch.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.ConfirmDialog, Is.False);
    }
    
    [Test]
    public async Task PatchSetting_UpdatesCorrectField_Celsius()
    {
        var patch = new UpdateUserSettingDto { Value = true };

        var resp = await _client.PatchAsJsonAsync("/api/userSettings/celsius", patch);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var fetch = await _client.GetAsync("/api/userSettings");
        var settings = await fetch.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.Celsius, Is.True);
    }
    
    [Test]
    public async Task PatchSetting_UpdatesCorrectField_DarkTheme()
    {
        var patch = new UpdateUserSettingDto { Value = false };

        var resp = await _client.PatchAsJsonAsync("/api/userSettings/darkTheme", patch);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var fetch = await _client.GetAsync("/api/userSettings");
        var settings = await fetch.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.DarkTheme, Is.False);
    }
    
    [Test]
    public async Task PatchSetting_UpdatesCorrectField_SecretMode()
    {
        var patch = new UpdateUserSettingDto { Value = false };

        var resp = await _client.PatchAsJsonAsync("/api/usersettings/secretmode", patch);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));

        var fetch = await _client.GetAsync("/api/userSettings");
        var settings = await fetch.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(settings, Is.Not.Null);
        Assert.That(settings.SecretMode, Is.False);
    }

    [Test]
    public async Task PatchSetting_InvalidName_ReturnsBadRequest()
    {
        var patch = new UpdateUserSettingDto { Value = true };
        var resp = await _client.PatchAsJsonAsync("/api/userSettings/invalidFlag", patch);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("Invalid setting name"));
        
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(); });
    }
}
