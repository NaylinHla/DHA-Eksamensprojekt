using Application.Interfaces.Infrastructure.MQTT;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserDevice.Request;
using Application.Services;
using Core.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.UserDeviceTests;

public class UserDeviceServiceTests
{
    private readonly Guid _otherUserId = Guid.NewGuid();
    private readonly Guid _userId = Guid.NewGuid();
    private Mock<IMqttPublisher> _mqttPublisherMock = null!;
    private UserDeviceService _service = null!;
    private Mock<IUserDeviceRepository> _userDeviceRepoMock = null!;

    [SetUp]
    public void Setup()
    {
        _userDeviceRepoMock = new Mock<IUserDeviceRepository>();
        _mqttPublisherMock = new Mock<IMqttPublisher>();
        _service = new UserDeviceService(_userDeviceRepoMock.Object, _mqttPublisherMock.Object);
    }

    private JwtClaims MockClaims(Guid id)
    {
        return new JwtClaims()
        {
            Id = id.ToString(),
            Role = "User",
            Email = "test@example.com",
            Exp = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:mm:ssZ")
        };
    }


    [Test]
    public void GetUserDeviceAsync_ShouldThrowUnauthorized_WhenDeviceNotOwned()
    {
        var deviceId = Guid.NewGuid();

        var device = new UserDevice
        {
            DeviceId = deviceId,
            UserId = _otherUserId,
            DeviceName = "Test Device",
            DeviceDescription = "Device Description",
            WaitTime = "60",
            CreatedAt = DateTime.UtcNow
        };

        _userDeviceRepoMock.Setup(r => r.GetUserDeviceByIdAsync(deviceId)).ReturnsAsync(device);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.GetUserDeviceAsync(deviceId, MockClaims(_userId)));
    }

    [Test]
    public void UpdateUserDeviceAsync_ShouldThrowUnauthorized_WhenDeviceNotOwned()
    {
        var deviceId = Guid.NewGuid();

        var device = new UserDevice
        {
            DeviceId = deviceId,
            UserId = _otherUserId,
            DeviceName = "Test Device",
            DeviceDescription = "Device Description",
            WaitTime = "60",
            CreatedAt = DateTime.UtcNow
        };

        _userDeviceRepoMock.Setup(r => r.GetUserDeviceByIdAsync(deviceId)).ReturnsAsync(device);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateUserDeviceAsync(deviceId, new UserDeviceEditDto(), MockClaims(_userId)));
    }

    [Test]
    public void DeleteUserDeviceAsync_ShouldThrowUnauthorized_WhenDeviceNotOwned()
    {
        var deviceId = Guid.NewGuid();

        var device = new UserDevice
        {
            DeviceId = deviceId,
            UserId = _otherUserId,
            DeviceName = "Test Device",
            DeviceDescription = "Device Description",
            WaitTime = "60",
            CreatedAt = DateTime.UtcNow
        };

        _userDeviceRepoMock.Setup(r => r.GetUserDeviceByIdAsync(deviceId)).ReturnsAsync(device);

        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.DeleteUserDeviceAsync(deviceId, MockClaims(_userId)));
    }

    [Test]
    public void UpdateDeviceFeed_ShouldThrowUnauthorized_WhenDeviceNotOwned()
    {
        // Arrange
        var deviceId = Guid.NewGuid();
        var dto = new AdminChangesPreferencesDto
        {
            DeviceId = deviceId.ToString(),
            Interval = "15"
        };

        var device = new UserDevice
        {
            DeviceId = deviceId,
            UserId = _otherUserId,
            DeviceName = "Test Device",
            DeviceDescription = "Device Description",
            WaitTime = "60",
            CreatedAt = DateTime.UtcNow
        };

        _userDeviceRepoMock.Setup(r => r.GetUserDeviceByIdAsync(deviceId)).ReturnsAsync(device);

        var claims = MockClaims(_userId);

        // Act & Assert
        Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            await _service.UpdateDeviceFeed(dto, claims));
    }
    
    [Test]
    public void UpdateDeviceFeed_ShouldThrowArgumentException_WhenDeviceIdIsNullOrEmpty()
    {
        // Arrange
        var dto = new AdminChangesPreferencesDto
        {
            DeviceId = "", // or null
            Interval = "15"
        };

        var claims = MockClaims(_userId);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(() =>
            _service.UpdateDeviceFeed(dto, claims));

        Assert.That(ex.Message, Is.EqualTo("DeviceId is required."));
    }
}