using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Application.Models.Dtos.RestDtos.UserDevice.Response;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.UserDeviceTests;

[TestFixture]
public class UserDeviceControllerTests : WebApplicationFactory<Program>
{
    [SetUp]
    public async Task Setup()
    {
        _client = CreateClient();

        // Seed the user and db with stuff
        _testUser = await MockObjects.SeedDbAsync(Services);

        var device = _testUser.UserDevices.First();
        _deviceId = device.DeviceId;

        var loginResp =
            await _client.PostAsJsonAsync("/api/auth/login", new { _testUser.Email, Password = "Secret25!" });
        loginResp.EnsureSuccessStatusCode();
        var loginDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(loginDto, Is.Not.Null);
        _jwt = loginDto.Jwt;
        _client.DefaultRequestHeaders.Add("Authorization", _jwt);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private HttpClient _client;
    private User _testUser;
    private string _jwt;
    private Guid _deviceId;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: false); });
    }

    // -------------------- GET: Get User Device --------------------

    [Test]
    public async Task GetUserDevice_InvalidId_ShouldReturnNotFound()
    {
        var resp = await _client.GetAsync(
            $"api/UserDevice/{UserDeviceController.GetUserDeviceRoute}?userDeviceId={Guid.NewGuid()}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task GetUserDevice_ShouldReturnDevice()
    {
        var resp = await _client.GetAsync(
            $"api/UserDevice/{UserDeviceController.GetUserDeviceRoute}?userDeviceId={_deviceId}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dto = await resp.Content.ReadFromJsonAsync<UserDeviceResponseDto>();
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto.DeviceId, Is.EqualTo(_deviceId));
    }

    // -------------------- GET: Get All User Devices --------------------


    [Test]
    public async Task GetAllUserDevices_ShouldReturnOk()
    {
        var resp = await _client.GetAsync($"api/UserDevice/{UserDeviceController.GetAllUserDevicesRoute}");
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllUserDevices_ShouldReturnEmpty_WhenNoDevicesExist()
    {
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.UserDevices.RemoveRange(db.UserDevices);
            await db.SaveChangesAsync();
        }

        var response = await _client.GetAsync($"api/UserDevice/{UserDeviceController.GetAllUserDevicesRoute}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var contentObject = await response.Content.ReadFromJsonAsync<List<UserDeviceResponseDto>>();
        Assert.That(contentObject, Is.Empty);
    }

    [Test]
    public async Task GetAllUserDevices_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        var clientWithoutJwt = CreateClient();
        var response =
            await clientWithoutJwt.GetAsync($"api/UserDevice/{UserDeviceController.GetAllUserDevicesRoute}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // -------------------- POST: Create User Device --------------------

    [Test]
    public async Task CreateUserDevice_ShouldPersistAllFields()
    {
        var dto = new UserDeviceCreateDto
        {
            DeviceName = "New Device",
            DeviceDescription = "Created from test",
            WaitTime = "900",
            Created = DateTime.UtcNow
        };

        var resp = await _client.PostAsJsonAsync($"api/UserDevice/{UserDeviceController.CreateUserDeviceRoute}",
            dto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var created = await resp.Content.ReadFromJsonAsync<UserDeviceResponseDto>();
        Assert.That(created, Is.Not.Null);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var dbDevice = await db.UserDevices.FindAsync(created.DeviceId);

        Assert.That(dbDevice, Is.Not.Null);
        Assert.Multiple(() =>
        {
            Assert.That(dbDevice.DeviceName, Is.EqualTo(dto.DeviceName));
            Assert.That(dbDevice.DeviceDescription, Is.EqualTo(dto.DeviceDescription));
            Assert.That(dbDevice.WaitTime, Is.EqualTo(dto.WaitTime));
            Assert.That(dbDevice.CreatedAt, Is.EqualTo(dto.Created).Within(TimeSpan.FromSeconds(1)));
            Assert.That(dbDevice.UserId.ToString(), Is.EqualTo(_testUser.UserId.ToString()));
        });
    }

    [Test]
    public async Task CreateUserDevice_MissingRequiredFields_ShouldReturnBadRequest()
    {
        var badDto = new { DeviceDescription = "Missing name and wait time" };

        var resp = await _client.PostAsJsonAsync($"api/UserDevice/{UserDeviceController.CreateUserDeviceRoute}",
            badDto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateUserDevice_ShouldDefaultWaitTime_WhenMissing()
    {
        var dto = new UserDeviceCreateDto
        {
            DeviceName = "No Wait Device",
            DeviceDescription = "Test fallback wait time",
            Created = DateTime.UtcNow
            // WaitTime is intentionally omitted
        };

        var resp = await _client.PostAsJsonAsync($"api/UserDevice/{UserDeviceController.CreateUserDeviceRoute}", dto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var created = await resp.Content.ReadFromJsonAsync<UserDeviceResponseDto>();
        Assert.That(created, Is.Not.Null);

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var dbDevice = await db.UserDevices.FindAsync(created.DeviceId);
        Assert.That(dbDevice, Is.Not.Null);

        Assert.That(dbDevice.WaitTime, Is.EqualTo("60")); // default fallback
    }


    // -------------------- PATCH: Edit User Device --------------------

    [Test]
    public async Task EditUserDevice_ShouldUpdateAllFields()
    {
        var updateDto = new UserDeviceEditDto
        {
            DeviceName = "Updated Device",
            DeviceDescription = "Updated description",
            WaitTime = "450"
        };

        var resp = await _client.PatchAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.EditUserDeviceRoute}?userDeviceId={_deviceId}", updateDto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updated = await resp.Content.ReadFromJsonAsync<UserDeviceResponseDto>();

        Assert.That(updated, Is.Not.Null, "Response DTO was null");
        Assert.Multiple(() =>
        {
            Assert.That(updated.DeviceName, Is.EqualTo(updateDto.DeviceName), "DeviceName was not updated");
            Assert.That(updated.DeviceDescription, Is.EqualTo(updateDto.DeviceDescription),
                "DeviceDescription was not updated");
            Assert.That(updated.WaitTime, Is.EqualTo(updateDto.WaitTime), "WaitTime was not updated");
        });
    }

    [Test]
    public async Task EditUserDevice_InvalidId_ShouldReturnNotFound()
    {
        var updateDto = new UserDeviceEditDto
        {
            DeviceName = "Should Fail",
            DeviceDescription = "Invalid",
            WaitTime = "100"
        };

        var resp = await _client.PatchAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.EditUserDeviceRoute}?userDeviceId={Guid.NewGuid()}", updateDto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // -------------------- DELETE: Delete User Device --------------------

    [Test]
    public async Task DeleteUserDevice_ShouldSucceed()
    {
        // Arrange: Get the deviceId of the first user device seeded in Setup
        var deviceId = _testUser.UserDevices.First().DeviceId;

        // Act: Send DELETE request to delete the user device
        var response = await _client.DeleteAsync(
            $"/api/UserDevice/{UserDeviceController.DeleteUserDeviceRoute}?userDeviceId={deviceId}"
        );

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Verify the device is no longer in the database
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var deletedDevice = db.UserDevices.FirstOrDefault(d => d.DeviceId == deviceId);

            Assert.That(deletedDevice, Is.Null, "Device should have been deleted from the database.");
        }

        var getResponse = await _client.GetAsync(
            $"api/UserDevice/{UserDeviceController.GetUserDeviceRoute}?userDeviceId={deviceId}"
        );

        // Assert: Device should not be found, expecting NotFound response
        Assert.That(getResponse.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }


    [Test]
    public async Task DeleteUserDevice_InvalidId_ShouldReturnNotFound()
    {
        var dto = new UserDeviceEditDto
        {
            DeviceName = "Doesn't Exist",
            DeviceDescription = "Nope",
            WaitTime = "300"
        };

        var resp = await _client.SendAsync(new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri =
                new Uri(
                    $"api/UserDevice/{UserDeviceController.DeleteUserDeviceRoute}?userDeviceId={Guid.NewGuid()}",
                    UriKind.Relative),
            Content = JsonContent.Create(dto)
        });

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    // -------------------- POST: Admin Changes Preferences --------------------

    // Currently calls actual Mqtt Client - Not Mocked
    /*
    [Test]
    public async Task AdminChangesPreferences_ShouldPersistWaitTimeChange_WhenValid()
    {
        // Arrange
        Guid deviceId;
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var device = new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = _testUser.UserId,
                DeviceName = "D2",
                DeviceDescription = "desc",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "600"
            };
            db.UserDevices.Add(device);
            await db.SaveChangesAsync();
            deviceId = device.DeviceId;
        }

        var dto = new AdminChangesPreferencesDto
        {
            DeviceId = deviceId.ToString(),
            Interval = "60"
        };

        // Act: Send POST request to update preferences
        var resp = await _client.PostAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.AdminChangesPreferencesRoute}", dto);

        // Assert: HTTP response is OK
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            var updated = await db.UserDevices.FindAsync(deviceId);
            Assert.That(updated, Is.Not.Null, "Device should still exist in DB");
            Assert.That(updated!.WaitTime, Is.EqualTo("60"),
                "DB should have the updated wait time");
        }
    }
    */

    [Test]
    public async Task AdminChangesPreferences_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        var client = CreateClient(); // no JWT
        var dto = new AdminChangesPreferencesDto
        {
            DeviceId = Guid.NewGuid().ToString(),
            Interval = "60"
        };

        var resp = await client.PostAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.AdminChangesPreferencesRoute}", dto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}