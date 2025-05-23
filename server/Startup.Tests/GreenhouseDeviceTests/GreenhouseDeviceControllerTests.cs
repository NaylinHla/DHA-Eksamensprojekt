using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Response;
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
using UserDevice = Core.Domain.Entities.UserDevice;

namespace Startup.Tests.GreenhouseDeviceTests;

[TestFixture]
public class GreenhouseDeviceControllerTests : WebApplicationFactory<Program>
{
    [SetUp]
    public async Task Setup()
    {
        _connManagerMock = new Mock<IConnectionManager>();
        
        _client = CreateClient();

        // Seed the user and db with stuff
        _testUser = await MockObjects.SeedDbAsync(Services);

        // Login to get JWT
        var loginResp = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { _testUser.Email, Password = "Secret25!" }
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
    }

    private HttpClient _client = null!;
    private User _testUser = null!;
    private string _jwt = null!;
    private Mock<IConnectionManager> _connManagerMock = null!;
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // First apply your default test config (seeds real DB, etc.)
            services.DefaultTestConfig();

            // Then replace IConnectionManager with our mock
            _connManagerMock = new Mock<IConnectionManager>();
            services.RemoveAll<IConnectionManager>();
            services.AddSingleton(_connManagerMock.Object);
        });
    }

    // -------------------- GET: GetSensorDataByDeviceId --------------------

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnOk()
    {
        Guid deviceId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var device = new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = _testUser.UserId,
                DeviceName = "D1",
                DeviceDescription = "desc",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600"
            };
            db.UserDevices.Add(device);
            await db.SaveChangesAsync();
            deviceId = device.DeviceId;
        }

        var resp = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={deviceId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnOk_WhenDeviceAndSensorDataExist()
    {
        Guid deviceId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var device = new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = _testUser.UserId,
                DeviceName = "D1",
                DeviceDescription = "desc",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600"
            };
            db.UserDevices.Add(device);

            // Add fake sensor data so that the service returns something
            db.SensorHistories.Add(new SensorHistory
            {
                SensorHistoryId = Guid.NewGuid(),
                DeviceId = device.DeviceId,
                Humidity = 50,
                Temperature = 22,
                Time = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
            deviceId = device.DeviceId;
        }

        var resp = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={deviceId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnNotFound_WhenDeviceIdDoesNotExist()
    {
        var response =
            await _client.GetAsync(
                $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetSensorHistoryByDeviceIdAndBroadcast_ShouldReturnUnauthorized_WhenDeviceDoesNotBelongToUser()
    {
        // Arrange: Create another user
        var anotherUser = MockObjects.GetUser(); // This gives a new user with a different ID
        using (var seedScope = Services.CreateScope())
        {
            var seedDb = seedScope.ServiceProvider.GetRequiredService<MyDbContext>();
            seedDb.Users.Add(anotherUser);
            await seedDb.SaveChangesAsync();
        }

        var deviceId = Guid.NewGuid();

        // Now seed a device owned by another user
        using (var seedScope = Services.CreateScope())
        {
            var db = seedScope.ServiceProvider.GetRequiredService<MyDbContext>();
            var device = new UserDevice
            {
                DeviceId = deviceId,
                UserId = anotherUser.UserId,
                DeviceName = "D1",
                DeviceDescription = "desc",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600"
            };
            db.UserDevices.Add(device);
            await db.SaveChangesAsync();
        }

        // Act: Try to access the device with _testUser's JWT (from Setup)
        var response = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={deviceId}"
        );

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        Assert.That(Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync())["title"]?.ToString(),
            Is.EqualTo("You do not own this device."));

    }

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnBadRequest_WhenJwtIsInvalid()
    {
        var clientWithoutJwt = CreateClient();
        clientWithoutJwt.DefaultRequestHeaders.Add("Authorization", "invalid-token");
        var response =
            await clientWithoutJwt.GetAsync(
                $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId=valid-id");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnBadRequest_WhenDeviceIdIsNotAGuid()
    {
        var response = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId=not-a-guid"
        );
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetSensorDataByDeviceId_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        var client = CreateClient();
        var response =
            await client.GetAsync(
                $"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={Guid.NewGuid()}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // -------------------- GET: GetRecentSensorDataForAllUserDevice --------------------

    [Test]
    public async Task GetRecentSensorDataForAllUserDevice_ShouldReturnOk()
    {
        var resp = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetRecentSensorDataForAllUserDeviceRoute}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetRecentSensorDataForAllUserDevice_ShouldReturnOk_WhenDataExists()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var device = new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = _testUser.UserId,
                DeviceName = "D3",
                DeviceDescription = "desc",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600"
            };
            db.UserDevices.Add(device);
            await db.SaveChangesAsync();
        }

        var resp = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetRecentSensorDataForAllUserDeviceRoute}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetRecentSensorDataForAllUserDevice_ShouldReturnNoContent_WhenNoDevicesExist()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.UserDevices.RemoveRange(db.UserDevices);
            await db.SaveChangesAsync();
        }

        var resp = await _client.GetAsync(
            $"api/GreenhouseDevice/{GreenhouseDeviceController.GetRecentSensorDataForAllUserDeviceRoute}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
    }

    [Test]
    public async Task GetRecentSensorDataForAllUserDevice_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        // Act: call without the required 'authorization' header
        var response = await CreateClient()
            .GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetRecentSensorDataForAllUserDeviceRoute}");

        // Assert: model binding fails because [FromHeader] authorization is missing → 400
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // -------------------- DELETE: DeleteData --------------------

    [Test]
    public async Task DeleteDataFromSpecificDevice_ShouldDeleteSensorDataFromDb()
    {
        // Arrange: get deviceId from the user seeded in Setup
        var deviceId = _testUser.UserDevices.First().DeviceId;

        // Ensure that sensor data exists for this device
        var sensorData = new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = deviceId,
            Humidity = 50,
            Temperature = 22,
            Time = DateTime.UtcNow
        };

        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.SensorHistories.Add(sensorData);
            await db.SaveChangesAsync();
        }

        // Ensure the sensor data exists before deletion
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var existingSensorData = await db.SensorHistories.FirstOrDefaultAsync(s => s.DeviceId == deviceId);
            Assert.That(existingSensorData, Is.Not.Null, "Sensor data should exist before deletion");
        }

        // Act: Perform DELETE request to remove data from the device
        var response = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={deviceId}"
        );

        // Assert: Check that the response is OK
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Assert: Check that the sensor data no longer exists in the database
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var deletedSensorData = await db.SensorHistories.FirstOrDefaultAsync(s => s.DeviceId == deviceId);
            Assert.That(deletedSensorData, Is.Null, "Sensor data should be deleted after the DELETE request");
        }
    }

    [Test]
    public async Task DeleteDataFromSpecificDevice_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        // Arrange: create a new unauthenticated client
        var deviceId = _testUser.UserDevices.First().DeviceId;
        var client = CreateClient(); // no JWT

        var response = await client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={deviceId}"
        );

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    [Test]
    public async Task DeleteDataFromSpecificDevice_ShouldReturnForbidden_WhenDeviceDoesNotBelongToUser()
    {
        // Arrange: create another user and a device they own
        var otherUser = MockObjects.GetUser();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.Users.Add(otherUser);
            db.UserDevices.Add(new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = otherUser.UserId,
                DeviceName = "ForeignDevice",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600",
                DeviceDescription = "behind tomato"
            });
            await db.SaveChangesAsync();
        }

        var foreignDeviceId = otherUser.UserDevices.First().DeviceId;

        // Act: attempt to delete with logged-in _testUser
        var response = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={foreignDeviceId}"
        );

        Assert.Multiple(async () =>
        {
            // Assert: should be forbidden
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
            Assert.That(Newtonsoft.Json.Linq.JObject.Parse(await response.Content.ReadAsStringAsync())["title"]?.ToString(),
                Is.EqualTo("You do not own this device."));
        });
    }
    
    [Test]
    public async Task DeleteDataFromSpecificDevice_ShouldReturnNotFound_WhenDeviceDoesNotExist()
    {
        var nonExistentDeviceId = Guid.NewGuid();

        var response = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={nonExistentDeviceId}"
        );

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task DeleteData_ShouldBroadcastAdminHasDeletedData_WhenAuthorized()
    {
        // Arrange: seed some sensor data so delete does something
        var deviceId = _testUser.UserDevices.First().DeviceId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.SensorHistories.Add(new SensorHistory
            {
                SensorHistoryId = Guid.NewGuid(),
                DeviceId = deviceId,
                Humidity = 10,
                Temperature = 20,
                Time = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Act
        var resp = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={deviceId}"
        );

        // Assert HTTP
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Assert broadcast
        _connManagerMock.Verify(cm =>
                cm.BroadcastToTopic(
                    StringConstants.Dashboard,
                    It.Is<AdminHasDeletedData>(e => e.eventType == nameof(AdminHasDeletedData))
                ),
            Times.Once);
    }
    
    [Test]
    public async Task DeleteData_ShouldNotBroadcastAndReturnForbidden_WhenNotOwner()
    {
        // Arrange: create foreign device for another user
        var other = MockObjects.GetUser();
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.Users.Add(other);
            db.UserDevices.Add(new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = other.UserId,
                DeviceName = "Foreign",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600",
                DeviceDescription = "x"
            });
            await db.SaveChangesAsync();
        }
        var foreignId = other.UserDevices.First().DeviceId;

        // Act
        var resp = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={foreignId}"
        );

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Forbidden));
        // no broadcast
        _connManagerMock.Verify(cm =>
                cm.BroadcastToTopic(It.IsAny<string>(), It.IsAny<AdminHasDeletedData>()),
            Times.Never);
    }

    // -------------------- Helper Class --------------------

    public class WrapperDto
    {
        public IEnumerable<GetAllUserDeviceDto> AllUserDevice { get; set; } = new List<GetAllUserDeviceDto>();
    }
}