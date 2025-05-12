using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Startup.Tests.UserDeviceTests;

public class UserDeviceRepositoryTests
{
    private MyDbContext _context = null!;
    private UserDeviceRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MyDbContext(options);
        _repository = new UserDeviceRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }
    
    [Test]
    public async Task CreateUserDeviceAsync_ShouldAddDeviceToDatabase()
    {
        var deviceId = Guid.NewGuid();
        var userDevice = new UserDevice
        {
            UserId = Guid.NewGuid(),
            DeviceName = "Created Device",
            DeviceDescription = "Desc",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "100"
        };

        var result = await _repository.CreateUserDeviceAsync(deviceId, userDevice);

        var deviceInDb = await _context.UserDevices.FindAsync(deviceId);

        Assert.That(deviceInDb, Is.Not.Null);
        Assert.That(deviceInDb!.DeviceName, Is.EqualTo("Created Device"));
        Assert.That(result.DeviceId, Is.EqualTo(deviceId));
    }


    [Test]
    public async Task GetUserDeviceByIdAsync_ShouldReturnDevice_WhenExists()
    {
        var deviceId = Guid.NewGuid();
        var userDevice = new UserDevice
        {
            DeviceId = deviceId,
            UserId = Guid.NewGuid(),
            DeviceName = "Test Device",
            DeviceDescription = "Test description",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "100"
        };

        _context.UserDevices.Add(userDevice);
        await _context.SaveChangesAsync();

        var result = await _repository.GetUserDeviceByIdAsync(deviceId);
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.DeviceId, Is.EqualTo(deviceId));
    }

    [Test]
    public async Task GetAllUserDevicesAsync_ShouldReturnList_WhenDevicesExist()
    {
        var userId = Guid.NewGuid();

        _context.UserDevices.AddRange(
            new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = userId,
                DeviceName = "A",
                DeviceDescription = "Desc A",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "100"
            },
            new UserDevice
            {
                DeviceId = Guid.NewGuid(),
                UserId = userId,
                DeviceName = "B",
                DeviceDescription = "Desc B",
                CreatedAt = DateTime.UtcNow,
                WaitTime = "200"
            }
        );

        await _context.SaveChangesAsync();

        var devices = await _repository.GetAllUserDevicesAsync(userId);

        Assert.That(devices, Has.Count.EqualTo(2));
    }

    [Test]
    public void GetUserDeviceOwnerUserIdAsync_ShouldThrow_WhenNotFound()
    {
        var nonexistentId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.GetUserDeviceOwnerUserIdAsync(nonexistentId);
        });

        Assert.That(ex!.Message, Does.Contain("Device not found"));
    }

    [Test]
    public async Task GetUserDeviceOwnerUserIdAsync_ShouldReturnUserId_WhenFound()
    {
        var userId = Guid.NewGuid();
        var deviceId = Guid.NewGuid();

        _context.UserDevices.Add(new UserDevice
        {
            DeviceId = deviceId,
            UserId = userId,
            DeviceName = "Device 1",
            DeviceDescription = "Test Desc",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "100"
        });

        await _context.SaveChangesAsync();

        var result = await _repository.GetUserDeviceOwnerUserIdAsync(deviceId);

        Assert.That(result, Is.EqualTo(userId));
    }
    
    [Test]
    public async Task DeleteUserDeviceAsync_ShouldRemoveDeviceFromDatabase()
    {
        var deviceId = Guid.NewGuid();
        var userDevice = new UserDevice
        {
            DeviceId = deviceId,
            UserId = Guid.NewGuid(),
            DeviceName = "Device to Delete",
            DeviceDescription = "Desc",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "100"
        };

        _context.UserDevices.Add(userDevice);
        await _context.SaveChangesAsync();

        await _repository.DeleteUserDeviceAsync(deviceId);

        var deletedDevice = await _context.UserDevices.FindAsync(deviceId);
        Assert.That(deletedDevice, Is.Null);
    }
    
    [Test]
    public void DeleteUserDeviceAsync_ShouldThrowNotFoundException_WhenDeviceDoesNotExist()
    {
        var nonExistentDeviceId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.DeleteUserDeviceAsync(nonExistentDeviceId);
        });

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Is.EqualTo("Device not found"));
    }

}