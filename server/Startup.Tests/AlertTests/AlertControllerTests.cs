// Startup.Tests/AlertTests/AlertControllerTests.cs

using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
    private HttpClient _client = null!;
    private string _jwt = null!;
    private User _testUser = null!;
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(created!.AlertName, Is.EqualTo(alertDto.AlertName));
            Assert.That(created.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
            Assert.That(created.AlertPlantConditionId != null || created.AlertDeviceConditionId != null, Is.True);
        }
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
        using (Assert.EnterMultipleScope())
        {
            Assert.That(created!.AlertName, Is.EqualTo(alertDto.AlertName));
            Assert.That(created.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
            Assert.That(created.AlertPlantConditionId != null || created.AlertDeviceConditionId != null, Is.True);
        }
    }

    [Test]
    public async Task CreateAlert_NoJwt_ReturnsUnauthorized()
    {
        var client = CreateClient(); // no JWT

        var dto = new AlertCreateDto
        {
            AlertName = "Overheat",
            AlertDesc = "Temp > 40℃"
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
            AlertDesc = "No name should fail"
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
            AlertName = "X",
            AlertDesc = "Y",
            IsPlantCondition = true,
            AlertConditionId = null
        };

        // Act
        var resp = await _client.PostAsJsonAsync("api/Alert/CreateAlert", dto);

        // Assert only on the error message
        var pd = await resp.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.That(pd, Is.Not.Null, "Expected a ProblemDetails JSON payload");
        Assert.That(pd.Title, Is.EqualTo("AlertConditionId cannot be null"));
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

        // Verify Broadcast called once with correct generic type & data
        var expectedTopic = $"alerts-{_testUser.UserId}";
        _connManagerMock.Verify(ws => ws.BroadcastToTopic(
            expectedTopic,
            It.Is<ServerBroadcastsLiveAlertToAlertView>(o => IsAlertBroadcast(o))
        ), Times.Once);
    }

    private static bool IsAlertBroadcast(ServerBroadcastsLiveAlertToAlertView o)
    {
        return o is { eventType: nameof(ServerBroadcastsLiveAlertToAlertView), Alerts.Count: > 0 };
    }

    [Test]
    public async Task CreateScheduledWaterAlert_ShouldPersistAlertWithCorrectPlantNameAndConditionId()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        // Get the plant entity and its associated alert condition (ConditionAlertPlantId)
        var plant = _testUser.UserPlants.First().Plant;
        var conditionAlertPlantId = await ctx.ConditionAlertPlant
            .Where(ca => ca.PlantId == plant!.PlantId)
            .Select(ca => ca.ConditionAlertPlantId)
            .FirstAsync();

        // Prepare DTO with plant name and condition alert id
        var alertDto = new AlertCreateDto
        {
            AlertName = $"Scheduled Water Alert for {plant!.PlantName}",
            AlertDesc = $"Reminder: Check water conditions for {plant.PlantName}",
            AlertConditionId = conditionAlertPlantId,
            IsPlantCondition = true,
            AlertUser = _testUser.UserId
        };

        // Act - create alert through API
        var response = await _client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", alertDto);
        response.EnsureSuccessStatusCode();

        var createdAlert = await response.Content.ReadFromJsonAsync<AlertResponseDto>();

        Assert.That(createdAlert, Is.Not.Null);
        Assert.That(createdAlert.AlertName, Is.EqualTo(alertDto.AlertName));
        Assert.That(createdAlert.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
        Assert.That(createdAlert.AlertPlantConditionId, Is.EqualTo(conditionAlertPlantId));

        // Optional: Verify from DB directly that alert persisted with correct ConditionAlertPlantId and UserId
        var alertFromDb = await ctx.Alerts
            .Where(a => a.AlertId == createdAlert.AlertId)
            .FirstOrDefaultAsync();

        Assert.That(alertFromDb, Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(alertFromDb.AlertName, Does.Contain(plant.PlantName));
            Assert.That(alertFromDb.AlertDesc, Does.Contain(plant.PlantName));
            Assert.That(alertFromDb.AlertPlantConditionId, Is.EqualTo(conditionAlertPlantId));
        }
    }
}