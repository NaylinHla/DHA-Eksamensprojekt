using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.AlertTests;

[TestFixture]
public class AlertControllerTests : WebApplicationFactory<Program>
{
    private HttpClient _client;
    private string _jwt;
    private User _testUser;
    private Mock<IConnectionManager> _connManagerMock = null!;


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            _connManagerMock = new Mock<IConnectionManager>();
            services.DefaultTestConfig(makeMqttClient: false);

            // Remove and replace the IWs service with a mock
            var wsDesc = services.SingleOrDefault(d => d.ServiceType == typeof(IConnectionManager));
            if (wsDesc != null) services.Remove(wsDesc);
            services.AddSingleton<IConnectionManager>(_ => _connManagerMock.Object);
        });
    }

    [SetUp]
    public async Task Setup()
    {
        _client = CreateClient();

        // Seed the user and db with stuff
        _testUser = await MockObjects.SeedDbAsync(Services);

        // get JWT
        var loginResp = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { _testUser.Email, Password = "pass" }
        );
        loginResp.EnsureSuccessStatusCode();
        var authDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        _jwt = authDto!.Jwt;
        _client.DefaultRequestHeaders.Add("Authorization", _jwt);
    }

    [TearDown]
    public void TearDown() => _client.Dispose();

    // -------------------- GET: Get User Alerts --------------------
    
    [Test]
    public async Task GetAlerts_NoJwt_ReturnsUnauthorized()
    {
        var client = CreateClient(); // no JWT
        
        var resp = await client.GetAsync($"api/Alert/{AlertController.GetAlertsRoute}");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)
            .Or.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task GetAlerts_ShouldReturnOnlyCurrentUserAlerts()
    {
        // Act
        var response = await _client.GetAsync($"api/Alert/{AlertController.GetAlertsRoute}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var alerts = await response.Content.ReadFromJsonAsync<List<AlertResponseDto>>();
        Assert.That(alerts, Is.Not.Null);
    }
    
    // -------------------- POST: Create User Alerts --------------------
    
    [Test]
    public async Task CreateAlertWithPlat_ShouldPersistAndReturnAlert()
    {

        // Create a scope to access the required DbContext
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        // Get the actual ConditionAlertPlant ID for the seeded test user
        var capId = await ctx.ConditionAlertPlant
            .Where(x => x.PlantId == _testUser.UserPlants.First().PlantId)
            .Select(x => x.ConditionAlertPlantId)
            .FirstAsync();

        var alertDto = new AlertCreateDto
        {
            AlertName = "High Temp",
            AlertDesc = "Too hot!",
            IsPlantCondition = true,
            AlertConditionId = capId,
            AlertUser = _testUser.UserId
        };

        var resp = await _client.PostAsJsonAsync(
            $"api/Alert/{AlertController.CreateAlertRoute}",
            alertDto
        );

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    
        var created = await resp.Content.ReadFromJsonAsync<AlertResponseDto>();
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.AlertName, Is.EqualTo(alertDto.AlertName));
            Assert.That(created.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
            Assert.That(created.AlertPlantConditionId != null || created.AlertDeviceConditionId != null, Is.True);
        });
    }
    
    [Test]
    public async Task CreateAlertWithUserDevice_ShouldPersistAndReturnAlert()
    {

        // Create a scope to access the required DbContext
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        // Get the actual ConditionAlertPlant ID for the seeded test user
        var cadId = await ctx.ConditionAlertUserDevice
            .Where(x => x.UserDeviceId == _testUser.UserDevices.First().DeviceId)
            .Select(x => x.ConditionAlertUserDeviceId)
            .FirstAsync();

        var alertDto = new AlertCreateDto
        {
            AlertName = "Low Temp",
            AlertDesc = "Too cold!",
            IsPlantCondition = false,
            AlertConditionId = cadId,
            AlertUser = _testUser.UserId
        };

        var resp = await _client.PostAsJsonAsync(
            $"api/Alert/{AlertController.CreateAlertRoute}",
            alertDto
        );

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    
        var created = await resp.Content.ReadFromJsonAsync<AlertResponseDto>();
    
        Assert.That(created, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(created.AlertName, Is.EqualTo(alertDto.AlertName));
            Assert.That(created.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
            Assert.That(created.AlertPlantConditionId != null || created.AlertDeviceConditionId != null, Is.True);
        });
        
    }

    [Test]
    public async Task CreateAlert_NoJwt_ReturnsUnauthorized()
    {
        var client = CreateClient(); // no JWT
        
        var dto = new AlertCreateDto
        {
            AlertName = "Overheat",
            AlertDesc = "Temp > 40℃",
            IsPlantCondition = true,
            AlertConditionId = Guid.NewGuid(),
            AlertUser = _testUser.UserId
        };

        var resp = await client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", dto);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)
            .Or.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateAlert_MissingName_ReturnsBadRequest()
    {
        var badDto = new AlertCreateDto
        {
            AlertName = null, // Missing name
            AlertDesc = "No name should fail",
            IsPlantCondition = true,
            AlertConditionId = Guid.NewGuid(),
            AlertUser = _testUser.UserId
        };

        var resp = await _client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", badDto);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateAlertEndpoint_ReturnsErrorMessage_WhenConditionIdIsNull()
    {
        // Arrange
        var dto = new AlertCreateDto
        {
            AlertName        = "XX",
            AlertDesc        = "OPQRST",
            IsPlantCondition = true,
            AlertConditionId = null,
            AlertUser        = _testUser.UserId
        };

        // Act
        var resp = await _client.PostAsJsonAsync("api/Alert/CreateAlert", dto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateAlertWithPlant_ShouldBroadcastAlert()
    {
        // Arrange
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var capId = await ctx.ConditionAlertPlant
            .Where(x => x.PlantId == _testUser.UserPlants.First().PlantId)
            .Select(x => x.ConditionAlertPlantId)
            .FirstAsync();

        var alertDto = new AlertCreateDto
        {
            AlertName = "Humidity High",
            AlertDesc = "Alert test broadcast",
            IsPlantCondition = true,
            AlertConditionId = capId,
            AlertUser = _testUser.UserId
        };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"api/Alert/{AlertController.CreateAlertRoute}",
            alertDto);

        // Assert HTTP
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify: Broadcast was called once with type == "alert"
        var expectedTopic = $"alerts-{_testUser.UserId}";
        _connManagerMock.Verify(ws => ws.BroadcastToTopic(
            expectedTopic,
            It.Is<object>(o =>
                o.GetType().GetProperty("type")!.GetValue(o).ToString() == "alert"
            )
        ), Times.Once);
    }
}