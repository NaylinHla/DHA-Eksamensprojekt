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
using UserDevice = Core.Domain.Entities.UserDevice;

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
            await _client.PostAsJsonAsync("/api/auth/login", new { _testUser.Email, Password = "pass" });
        loginResp.EnsureSuccessStatusCode();
        var loginDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        _jwt = loginDto!.Jwt;
        _client.DefaultRequestHeaders.Add("Authorization", _jwt);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private HttpClient _client;
    private User _testUser = null!;
    private string _jwt = null!;
    private Guid _deviceId;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(s => s.DefaultTestConfig());
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
        Assert.Multiple(() =>
        {
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.DeviceId, Is.EqualTo(_deviceId));
        });
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
    public async Task CreateUserDevice_ShouldSucceed()
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
        Assert.Multiple(() =>
        {
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.DeviceName, Is.EqualTo(dto.DeviceName));
        });
    }

    // -------------------- PATCH: Edit User Device --------------------

    [Test]
    public async Task EditUserDevice_ShouldUpdate()
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
        Assert.That(updated!.DeviceName, Is.EqualTo(updateDto.DeviceName));
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

        // Assert: Verify that the device was successfully deleted (HTTP 200 OK)
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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

    // -------------------- POST: Create User Device Missing Required Fields --------------------

    [Test]
    public async Task CreateUserDevice_MissingRequiredFields_ShouldReturnBadRequest()
    {
        var badDto = new { DeviceDescription = "Missing name and wait time" };

        var resp = await _client.PostAsJsonAsync($"api/UserDevice/{UserDeviceController.CreateUserDeviceRoute}",
            badDto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    // -------------------- POST: Admin Changes Preferences --------------------

    [Test]
    public async Task AdminChangesPreferences_ShouldReturnOk()
    {
        string deviceId;
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
            deviceId = device.DeviceId.ToString();
        }

        var dto = new AdminChangesPreferencesDto
        {
            DeviceId = deviceId,
            Interval = "60"
        };

        var resp = await _client.PostAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.AdminChangesPreferencesRoute}", dto);
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

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