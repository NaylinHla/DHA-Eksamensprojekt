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

namespace Startup.Tests.AlertConditionTests;

public class AlertConditionControllerTests : WebApplicationFactory<Program>
{
    private HttpClient _client = null!;
    private string _jwt = null!;
    private User _testUser = null!;
    private MyDbContext _ctx = null!;
    private Mock<IConnectionManager> _wsMock = null!;
    private IServiceScope _scope = null!;

    // Error message constants
    private const string AlertConditionNotFound = "Alert Condition not found.";
    private const string AlertDupeConditionDevice = "A condition with the same sensor and logic already exists.";
    private const string UnauthorizedAlertConditionAccess = "You do not own this alert condition.";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            _wsMock = new Mock<IConnectionManager>();
            services.DefaultTestConfig(makeMqttClient: false);
            var desc = services.Single(d => d.ServiceType == typeof(IConnectionManager));
            services.Remove(desc);
            services.AddSingleton(_wsMock.Object);
        });
    }

    [SetUp]
    public async Task Setup()
    {
        _scope = Services.CreateScope();
        _ctx = _scope.ServiceProvider.GetRequiredService<MyDbContext>();

        _client = CreateClient();

        // Seed the user and db with initial data (including one alert plant and one alert device)
        _testUser = await MockObjects.SeedDbAsync(Services);

        // Login to get JWT
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
    public void TearDown()
    {
        _client.Dispose();
        _scope.Dispose();
    }

    // --- GetConditionAlertPlant ---

    [Test]
    public async Task GetConditionAlertPlant_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantRoute}?plantId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetConditionAlertPlant_OtherUsersPlant_ShouldReturnUnauthorized()
    {
        // Arrange: seed another user's plant + alert
        var otherUser = MockObjects.GetUser();
        var otherPlant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = "OtherPlant",
            PlantType = "Herb",
            Planted = DateTime.UtcNow.AddMonths(-1),
            LastWatered = DateTime.UtcNow,
            WaterEvery = 5,
            IsDead = false,
            PlantNotes = "Test notes"
        };
        _ctx.Users.Add(otherUser);
        _ctx.Plants.Add(otherPlant);
        _ctx.UserPlants.Add(new UserPlant { UserId = otherUser.UserId, PlantId = otherPlant.PlantId });
        _ctx.ConditionAlertPlant.Add(new ConditionAlertPlant
        {
            ConditionAlertPlantId = Guid.NewGuid(),
            PlantId = otherPlant.PlantId,
            WaterNotify = true,
            IsDeleted = false
        });
        await _ctx.SaveChangesAsync();

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantRoute}?plantId={otherPlant.PlantId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task GetConditionAlertPlant_SeededCondition_ShouldReturnSeededDto()
    {
        // Arrange
        var seededPlantId = _testUser.UserPlants.First().PlantId;
        var seededCondition = await _ctx.ConditionAlertPlant
            .Where(c => c.PlantId == seededPlantId && !c.IsDeleted)
            .FirstAsync();

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantRoute}?plantId={seededPlantId}");

        // Assert
        resp.EnsureSuccessStatusCode();
        var dto = await resp.Content.ReadFromJsonAsync<ConditionAlertPlantResponseDto>();
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto!.ConditionAlertPlantId, Is.EqualTo(seededCondition.ConditionAlertPlantId));
    }

    // --- GetConditionAlertPlants ---

    [Test]
    public async Task GetConditionAlertPlants_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantsRoute}?userId={_testUser.UserId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetConditionAlertPlants_User1TriesToAccessUser2Conditions_ShouldReturnForbidden()
    {
        // Arrange: seed second user
        var user2 = await MockObjects.SeedDbAsync(Services);

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantsRoute}?userId={user2.UserId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task GetConditionAlertPlants_WithConditions_ShouldReturnList()
    {
        // Arrange
        var otherPlant = new Plant
        {
            PlantId = Guid.NewGuid(),
            PlantName = "OtherPlant",
            PlantType = "Herb",
            Planted = DateTime.UtcNow.AddMonths(-1),
            LastWatered = DateTime.UtcNow,
            WaterEvery = 5,
            IsDead = false,
            PlantNotes = "Test notes"
        };

        // Add new plant and associate it with the test user
        _ctx.Plants.Add(otherPlant);
        _ctx.UserPlants.Add(new UserPlant
        {
            UserId = _testUser.UserId,
            PlantId = otherPlant.PlantId
        });
        await _ctx.SaveChangesAsync();

        // Create alert condition for the new plant
        await _client.PostAsJsonAsync($"/api/AlertCondition/{AlertConditionController.CreateConditionAlertPlantRoute}",
            new ConditionAlertPlantCreateDto { PlantId = otherPlant.PlantId });

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertPlantsRoute}?userId={_testUser.UserId}");

        // Assert
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<List<ConditionAlertPlantResponseDto>>();
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Count, Is.EqualTo(2));
    }

    // --- CreateConditionAlertPlant ---

    [Test]
    public async Task CreateConditionAlertPlant_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();
        var dto = new ConditionAlertPlantCreateDto { PlantId = _testUser.UserPlants.First().PlantId };

        // Act
        var resp = await client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertPlantRoute}", dto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized).Or.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateConditionAlertPlant_MissingDto_ShouldReturnBadRequestWithCorrectErrorMessage()
    {
        // Arrange:
        var invalidDto = new { };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertPlantRoute}", invalidDto);

        // Assert: Check the status code
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("The PlantId is required."));
    }
    
    [Test]
    public async Task CreateConditionAlertPlant_AsUser1_ForUser2Plant_ShouldReturnForbidden()
    {
        // Arrange
        var testUser2 = await MockObjects.SeedDbAsync(Services);
        var user2Plant = _ctx.UserPlants
            .Include(up => up.Plant)
            .FirstOrDefault(up => up.UserId == testUser2.UserId)?.Plant;

        if (user2Plant == null)
        {
            Assert.Fail("User 2 does not have a plant in the database.");
        }

        var conditionDto = new ConditionAlertPlantCreateDto
        {
            PlantId = user2Plant!.PlantId,
        };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertPlantRoute}", conditionDto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    // --- DeleteConditionAlertPlant ---

    [Test]
    public async Task DeleteConditionAlertPlant_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertPlantRoute}?conditionId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteConditionAlertPlant_NonexistentId_ShouldReturnNotFound()
    {
        // Arrange: non-existent ID

        // Act
        var resp = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertPlantRoute}?conditionId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(AlertConditionNotFound));
    }

    [Test]
    public async Task DeleteConditionAlertPlant_AsUser1_ShouldReturnForbidden()
    {
        // Arrange:
        var testUser42 = await MockObjects.SeedDbAsync(Services); // User 2 is seeded with data
        var user42PlantCondition = _ctx.ConditionAlertPlant
            .FirstOrDefault(c => c.PlantId == testUser42.UserPlants.First().PlantId);

        if (user42PlantCondition == null)
        {
            Assert.Fail("User 2 does not have a plant condition in the database.");
        }

        // Act
        var deleteResponse = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertPlantRoute}?conditionId={user42PlantCondition?.ConditionAlertPlantId}");

        // Assert
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var body = await deleteResponse.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    
    [Test]
    public async Task DeleteConditionAlertPlant_ValidId_ShouldSoftDelete()
    {
        // Arrange
        var pid = _testUser.UserPlants.First().PlantId;

        var conditionAlertPlant = await _ctx.ConditionAlertPlant
            .FirstOrDefaultAsync(c => c.PlantId == pid);

        if (conditionAlertPlant == null)
        {
            Assert.Fail("Condition alert for the plant not found.");
        }

        // Act
        var del = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertPlantRoute}?conditionId={conditionAlertPlant?.ConditionAlertPlantId}");
        del.EnsureSuccessStatusCode();


        // Assert: Check that the sensor data no longer exists in the database
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var inDb = await db.ConditionAlertPlant.FindAsync(conditionAlertPlant?.ConditionAlertPlantId);
        Assert.That(inDb!.IsDeleted, Is.True);
    }


    // --- GetConditionAlertUserDevice ---

    [Test]
    public async Task GetConditionAlertUserDevice_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDeviceRoute}?userDeviceId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetConditionAlertUserDevice_OtherUsersDevice_ShouldReturnUnauthorized()
    {
        // Arrange: seed other user + device + alert
        var otherUser = MockObjects.GetUser();
        var otherDevice = new UserDevice
        {
            DeviceId = Guid.NewGuid(),
            DeviceName = "OtherDevice",
            DeviceDescription = "Desc",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "10",
            UserId = otherUser.UserId
        };
        _ctx.Users.Add(otherUser);
        _ctx.UserDevices.Add(otherDevice);
        await _ctx.SaveChangesAsync();

        _ctx.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
        {
            ConditionAlertUserDeviceId = Guid.NewGuid(),
            UserDeviceId = otherDevice.DeviceId,
            SensorType = "Temp",
            Condition = "<=30",
            IsDeleted = false
        });
        await _ctx.SaveChangesAsync();

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDeviceRoute}?userDeviceId={otherDevice.DeviceId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task GetConditionAlertUserDevice_SeededCondition_ShouldReturnSeededDto()
    {
        // Arrange
        var seededDeviceId = _testUser.UserDevices.First().DeviceId;
        var seededCondition = await _ctx.ConditionAlertUserDevice
            .Where(c => c.UserDeviceId == seededDeviceId && !c.IsDeleted)
            .FirstAsync();

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDeviceRoute}?userDeviceId={seededDeviceId}");

        // Assert
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<List<ConditionAlertUserDeviceResponseDto>>();
        Assert.That(list, Is.Not.Null);
        Assert.That(list!.Count, Is.EqualTo(1));
        var dto = list.First();
        Assert.That(dto.ConditionAlertUserDeviceId, Is.EqualTo(seededCondition.ConditionAlertUserDeviceId));
    }

    // --- GetConditionAlertUserDevices ---

    [Test]
    public async Task GetConditionAlertUserDevices_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDevicesRoute}?userId={_testUser.UserId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetConditionAlertUserDevices_User1TriesToAccessUser2_ShouldReturnForbidden()
    {
        // Arrange
        var user2 = await MockObjects.SeedDbAsync(Services);

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDevicesRoute}?userId={user2.UserId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task GetConditionAlertUserDevices_NoConditions_ShouldReturnEmptyList()
    {
        // Arrange
        var first = await _ctx.ConditionAlertUserDevice
            .FirstAsync(c => c.UserDeviceId == _testUser.UserDevices.First().DeviceId);
        _ctx.ConditionAlertUserDevice.Remove(first);
        await _ctx.SaveChangesAsync();

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDevicesRoute}?userId={_testUser.UserId}"
        );
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<List<ConditionAlertUserDeviceResponseDto>>();

        // Assert
        Assert.That(list, Is.Empty);
    }


    [Test]
    public async Task GetConditionAlertUserDevices_WithConditions_ShouldReturnList()
    {
        // Arrange
        var dev = _testUser.UserDevices.First().DeviceId;
        await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}",
            new ConditionAlertUserDeviceCreateDto
                { UserDeviceId = dev.ToString(), SensorType = "Temperature", Condition = "=>30" });
        await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}",
            new ConditionAlertUserDeviceCreateDto
                { UserDeviceId = dev.ToString(), SensorType = "Temperature", Condition = "=>1" });

        // Act
        var resp = await _client.GetAsync(
            $"/api/AlertCondition/{AlertConditionController.GetConditionAlertUserDevicesRoute}?userId={_testUser.UserId}"
        );
        resp.EnsureSuccessStatusCode();
        var list = await resp.Content.ReadFromJsonAsync<List<ConditionAlertUserDeviceResponseDto>>();

        // Assert
        Assert.That(list!.Count, Is.EqualTo(3));
    }

    // --- CreateConditionAlertUserDevice ---

    [Test]
    public async Task CreateConditionAlertUserDevice_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}", new { });

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateConditionAlertUserDevice_InvalidModel_ShouldReturnBadRequest()
    {
        // Arrange
        var bad = new { UserDeviceId = "", SensorType = "X", Condition = "??" };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}", bad);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task CreateConditionAlertUserDevice_AsUser1_ForUser2Device_ShouldReturnForbidden()
    {
        // Arrange
        var testUser2 = await MockObjects.SeedDbAsync(Services);
        var user2Device = _ctx.UserDevices.FirstOrDefault(ud => ud.UserId == testUser2.UserId); // Get User 2's device
        
        if (user2Device == null)
        {
            Assert.Fail("User 2 does not have a device in the database.");
        }
        
        var conditionDto = new ConditionAlertUserDeviceCreateDto
        {
            UserDeviceId = user2Device!.DeviceId.ToString(),
            SensorType = "Temperature",
            Condition = "=>20"
        };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}", conditionDto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task CreateConditionAlertUserDevice_ValidDto_ShouldPersist()
    {
        // Arrange
        var dev = _testUser.UserDevices.First().DeviceId;
        var dto = new ConditionAlertUserDeviceCreateDto
            { UserDeviceId = dev.ToString(), SensorType = "AirPressure", Condition = "=>5" };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}", dto);
        resp.EnsureSuccessStatusCode();
        var created = await resp.Content.ReadFromJsonAsync<ConditionAlertUserDeviceResponseDto>();

        // Assert
        var inDb = await _ctx.ConditionAlertUserDevice.FindAsync(created!.ConditionAlertUserDeviceId);
        Assert.That(inDb, Is.Not.Null);
    }

    // --- EditConditionAlertUserDevice ---

    [Test]
    public async Task EditConditionAlertUserDevice_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.PatchAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.EditConditionAlertUserDeviceRoute}", new { });

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task EditConditionAlertUserDevice_NotFoundId_ShouldReturnNotFound()
    {
        // Arrange
        var dto = new ConditionAlertUserDeviceEditDto
        {
            ConditionAlertUserDeviceId = Guid.NewGuid().ToString(),
            UserDeviceId = _testUser.UserDevices.First().DeviceId.ToString(),
            SensorType = "AirQuality",
            Condition = "=>1"
        };

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.EditConditionAlertUserDeviceRoute}", dto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task UpdateConditionAlertUserDevice_AsUser1_ForUser2Condition_ShouldReturnForbidden()
    {
        // Arrange
        var testUser2 = await MockObjects.SeedDbAsync(Services);
    
        var user2Condition = _ctx.ConditionAlertUserDevice
            .Include(c => c.UserDevice)
            .FirstOrDefault(c => c.UserDevice != null && c.UserDevice.UserId == testUser2.UserId);

        if (user2Condition == null)
        {
            Assert.Fail("User 2 does not have a ConditionAlertUserDevice in the database.");
        }

        var updateDto = new ConditionAlertUserDeviceEditDto
        {
            ConditionAlertUserDeviceId = user2Condition!.ConditionAlertUserDeviceId.ToString(),
            UserDeviceId = user2Condition.UserDeviceId.ToString(),
            Condition = "=>30",
            SensorType = user2Condition.SensorType
        };

        // Act: 
        var resp = await _client.PatchAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.EditConditionAlertUserDeviceRoute}", updateDto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    // --- DeleteConditionAlertUserDevice ---

    [Test]
    public async Task DeleteConditionAlertUserDevice_NoJwt_ShouldReturnUnauthorized()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var resp = await client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertUserDeviceRoute}?conditionId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteConditionAlertUserDevice_NotFound_ShouldReturnNotFound()
    {
        // Arrange: Set up the condition ID for deletion
        var resp = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertUserDeviceRoute}?conditionId={Guid.NewGuid()}");

        // Assert: Ensure the response status is NotFound
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        var body = await resp.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(AlertConditionNotFound));
    }
    
    [Test]
    public async Task DeleteConditionAlertUserDevice_AsUser1_ShouldReturnForbidden()
    {
        // Arrange:
        var testUser2 = await MockObjects.SeedDbAsync(Services); // User 2 is seeded with data
        var user2DeviceCondition = _ctx.ConditionAlertUserDevice
            .FirstOrDefault(c => c.UserDevice != null && c.UserDevice.UserId == testUser2.UserId);

        if (user2DeviceCondition == null)
        {
            Assert.Fail("User 2 does not have a condition in the database.");
        }

        // Act:
        var deleteResponse = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertUserDeviceRoute}?conditionId={user2DeviceCondition?.ConditionAlertUserDeviceId}");

        // Assert:
        Assert.That(deleteResponse.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));

        var body = await deleteResponse.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain(UnauthorizedAlertConditionAccess));
    }

    [Test]
    public async Task DeleteConditionAlertUserDevice_ValidId_ShouldSoftDelete()
    {
        // Arrange
        var dev = _testUser.UserDevices.First().DeviceId;
        var create = await _client.PostAsJsonAsync(
            $"/api/AlertCondition/{AlertConditionController.CreateConditionAlertUserDeviceRoute}",
            new ConditionAlertUserDeviceCreateDto
                { UserDeviceId = dev.ToString(), SensorType = "Humidity", Condition = "<=0" });
        var created = await create.Content.ReadFromJsonAsync<ConditionAlertUserDeviceResponseDto>();

        // Act
        var del = await _client.DeleteAsync(
            $"/api/AlertCondition/{AlertConditionController.DeleteConditionAlertUserDeviceRoute}?conditionId={created!.ConditionAlertUserDeviceId}"
        );
        del.EnsureSuccessStatusCode();

        // Assert
        var inDb = await _ctx.ConditionAlertUserDevice.FindAsync(created.ConditionAlertUserDeviceId);
        Assert.That(inDb!.IsDeleted, Is.True);
    }
}