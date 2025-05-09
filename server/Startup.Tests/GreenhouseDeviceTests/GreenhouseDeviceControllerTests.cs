using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Response;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
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
        _client = CreateClient();

        // Seed test user
        _testUser = MockObjects.GetUser();
        using var seedScope = Services.CreateScope();
        var seedDb = seedScope.ServiceProvider.GetRequiredService<MyDbContext>();
        seedDb.Users.Add(_testUser);
        await seedDb.SaveChangesAsync();

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
    }

    private HttpClient _client = null!;
    private User _testUser = null!;
    private string _jwt = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(s => s.DefaultTestConfig());
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
    public async Task DeleteDataFromSpecificDevice_ShouldReturnOk()
    {
        // Arrange: get deviceId from the user seeded in Setup
        var deviceId = _testUser.UserDevices.First().DeviceId;

        var response = await _client.DeleteAsync(
            $"/api/GreenhouseDevice/{GreenhouseDeviceController.DeleteDataRoute}?deviceId={deviceId}"
        );

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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


    // -------------------- Helper Class --------------------

    public class WrapperDto
    {
        public IEnumerable<GetAllUserDeviceDto> AllUserDevice { get; set; } = new List<GetAllUserDeviceDto>();
    }
}