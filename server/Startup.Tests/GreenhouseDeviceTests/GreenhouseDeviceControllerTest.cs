using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NUnit.Framework;
using Startup.Tests.TestUtils;
using UserDevice = Core.Domain.Entities.UserDevice;
using Api.Rest.Controllers;

namespace Startup.Tests.GreenhouseDeviceTests
{
    [TestFixture]
    public class GreenhouseDeviceControllerTest : WebApplicationFactory<Program>
    {
        private HttpClient _client = null!;
        private User _testUser = null!;
        private string _jwt = null!;


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
                new { Email = _testUser.Email, Password = "pass" }
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

        protected override void ConfigureWebHost(IWebHostBuilder builder)
            => builder.ConfigureServices(s => s.DefaultTestConfig());

        [Test]
        public async Task GetAllUserDevices_ShouldReturnOk()
        {
            var resp = await _client.GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetAllUserDevicesRoute}");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task GetSensorDataByDeviceId_ShouldReturnOk()
        {
            // Seed a device
            Guid deviceId;
            using(var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var device = new UserDevice {
                    DeviceId = Guid.NewGuid(),
                    UserId = _testUser.UserId,
                    DeviceName = "D1",
                    DeviceDescription = "desc",
                    CreatedAt = DateTime.UtcNow
                };
                db.UserDevices.Add(device);
                await db.SaveChangesAsync();
                deviceId = device.DeviceId;
            }

            var resp = await _client.GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={deviceId}");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task AdminChangesPreferences_ShouldReturnOk()
        {
            // Seed a device
            string deviceId;
            using(var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                var device = new UserDevice {
                    DeviceId = Guid.NewGuid(),
                    UserId = _testUser.UserId,
                    DeviceName = "D2",
                    DeviceDescription = "desc",
                    CreatedAt = DateTime.UtcNow
                };
                db.UserDevices.Add(device);
                await db.SaveChangesAsync();
                deviceId = device.DeviceId.ToString();
            }

            // New DTO signature: all strings
            var dto = new AdminChangesPreferencesDto {
                DeviceId = deviceId,
                Unit     = "Celsius",
                Interval = "60"
            };

            var resp = await _client.PostAsJsonAsync(
                $"api/GreenhouseDevice/{GreenhouseDeviceController.AdminChangesPreferencesRoute}", dto
            );
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task DeleteData_ShouldReturnOk()
        {
            var resp = await _client.DeleteAsync("api/GreenhouseDevice/DeleteData");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }
        
        [Test]
        public async Task GetAllUserDevices_ShouldReturnBadRequest_WhenNoJwtProvided()
        {
            var clientWithoutJwt = CreateClient();
            var response = await clientWithoutJwt.GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetAllUserDevicesRoute}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task GetSensorDataByDeviceId_ShouldReturnNotFound_WhenDeviceIdDoesNotExist()
        {
            var nonExistentDeviceId = Guid.NewGuid(); // Random ID, not saved in the DB
            var response = await _client.GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetSensorDataRoute}?deviceId={nonExistentDeviceId}");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        }
        
        [Test]
        public async Task GetAllUserDevices_ShouldReturnEmpty_WhenNoDevicesExist()
        {
            // Remove all devices before testing
            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
                db.UserDevices.RemoveRange(db.UserDevices);
                await db.SaveChangesAsync();
            }

            var response = await _client.GetAsync($"api/GreenhouseDevice/{GreenhouseDeviceController.GetAllUserDevicesRoute}");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            
            // Define a wrapper DTO for the response format
            var contentObject = await response.Content.ReadFromJsonAsync<WrapperDto>();

            // Assert that the list inside 'allUserDevice' is empty
            Assert.That(contentObject!.AllUserDevice, Is.Empty); // Ensure the list is empty
        }

        // Wrapper class for the response format
        public class WrapperDto
        {
            public IEnumerable<GetAllUserDeviceDto> AllUserDevice { get; set; } = new List<GetAllUserDeviceDto>();
        }

    }
}
