using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Startup.Tests.GreenhouseDeviceTests;

public class GreenhouseDeviceRepositoryTests
{
    private MyDbContext _context = null!;
    private GreenhouseDeviceRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new MyDbContext(options);
        _repository = new GreenhouseDeviceRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void GetDeviceByIdAsync_ShouldThrowNotFoundException_WhenDeviceNotFound()
    {
        var nonExistingId = Guid.NewGuid();

        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.GetDeviceOwnerUserId(nonExistingId);
        });

        Assert.That(ex!.Message, Does.Contain("not found"));
    }

    [Test]
    public async Task GetUserByDeviceId_ShouldReturnUser_WhenDeviceExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Hash = "hash",
            Salt = "salt",
            Role = "User",
            Country = "Wonderland"
        };

        var deviceId = Guid.NewGuid();
        var userDevice = new UserDevice
        {
            DeviceId = deviceId,
            UserId = userId,
            DeviceName = "Test",
            DeviceDescription = "Test",
            CreatedAt = DateTime.Now,
            WaitTime = "600"
        };

        _context.Users.Add(user);
        _context.UserDevices.Add(userDevice);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByDeviceId(deviceId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.UserId, Is.EqualTo(userId));
    }

    [Test]
    public void GetUserByDeviceId_ShouldThrow_WhenDeviceNotFound()
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.GetUserByDeviceId(Guid.NewGuid());
        });

        Assert.That(ex!.Message, Is.EqualTo("Device not found"));
    }

    [Test]
    public void GetRecentSensorHistory_ShouldReturnOrderedList_ByDescendingTime()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var deviceId = Guid.NewGuid();

        var sensorData = new List<SensorHistory>
        {
            new()
            {
                SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 20, Humidity = 40,
                Time = now.AddMinutes(-10)
            },
            new()
            {
                SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 22, Humidity = 45, Time = now
            },
            new()
            {
                SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 21, Humidity = 42,
                Time = now.AddMinutes(-5)
            }
        };

        _context.SensorHistories.AddRange(sensorData);
        _context.SaveChanges();

        // Act
        var result = _repository.GetRecentSensorHistory();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result[0].Time, Is.EqualTo(now)); // Most recent
        Assert.That(result[1].Time, Is.EqualTo(now.AddMinutes(-5)));
        Assert.That(result[2].Time, Is.EqualTo(now.AddMinutes(-10))); // Oldest
    }

    [Test]
    public Task GetSensorHistoryByDeviceIdAsync_ShouldThrow_WhenDeviceNotFound()
    {
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await _repository.GetSensorHistoryByDeviceIdAsync(Guid.NewGuid());
        });

        Assert.That(ex!.Message, Is.EqualTo("Device not found"));
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetSensorHistoryByDeviceIdAsync_ShouldFilterByTimeRange()
    {
        var deviceId = Guid.NewGuid();
        var device = new UserDevice
        {
            DeviceId = deviceId,
            DeviceName = "FilterTest",
            CreatedAt = DateTime.UtcNow,
            DeviceDescription = "In the downstairs",
            WaitTime = "600"
        };
        _context.UserDevices.Add(device);

        var now = DateTime.UtcNow;

        var sensorHistories = new List<SensorHistory>
        {
            new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 10, Time = now.AddDays(-2) },
            new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 20, Time = now.AddDays(-1) },
            new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 30, Time = now }
        };

        _context.SensorHistories.AddRange(sensorHistories);
        await _context.SaveChangesAsync();

        var from = now.AddDays(-1); // Include the record from this exact date
        var to = now; // Include the record from this exact time

        // Act
        var result = await _repository.GetSensorHistoryByDeviceIdAsync(deviceId, from, to);

        // Assert
        // Expecting two records because the time range is inclusive
        Assert.That(result.Single().SensorHistoryRecords.Count, Is.EqualTo(2));
    
        // The first record should be included because it's exactly from 'now.AddDays(-1)'
        Assert.That(result.Single().SensorHistoryRecords[0].Temperature, Is.EqualTo(20)); 
    
        // The second record should be included because it's exactly from 'now'
        Assert.That(result.Single().SensorHistoryRecords[1].Temperature, Is.EqualTo(30));
    }
    
    [Test]
    public async Task GetLatestSensorDataForUserDevicesAsync_ShouldReturnLatestSensorDataForUserDevices()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            UserId = userId,
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Hash = "hash",
            Salt = "salt",
            Role = "User",
            Country = "Wonderland"
        };

        var deviceId = Guid.NewGuid();
        var device = new UserDevice
        {
            DeviceId = deviceId,
            UserId = userId,
            DeviceName = "Test Device",
            DeviceDescription = "Test Description",
            CreatedAt = DateTime.UtcNow,
            WaitTime = "600"
        };

        var sensorHistories = new List<SensorHistory>
        {
            new SensorHistory
            {
                SensorHistoryId = Guid.NewGuid(),
                DeviceId = deviceId,
                Temperature = 22,
                Humidity = 40,
                Time = DateTime.UtcNow.AddMinutes(-15)
            },
            new SensorHistory
            {
                SensorHistoryId = Guid.NewGuid(),
                DeviceId = deviceId,
                Temperature = 25,
                Humidity = 45,
                Time = DateTime.UtcNow.AddMinutes(-5)
            }
        };

        _context.Users.Add(user);
        _context.UserDevices.Add(device);
        _context.SensorHistories.AddRange(sensorHistories);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetLatestSensorDataForUserDevicesAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count, Is.EqualTo(1));
    
        // Ensure the most recent sensor data is returned (should be the second sensor history with temperature 25)
        Assert.That(result[0].Temperature, Is.EqualTo(25));
        Assert.That(result[0].Humidity, Is.EqualTo(45));
        Assert.That(result[0].Time, Is.EqualTo(sensorHistories[1].Time)); // Latest time
    }

    [Test]
    public async Task AddSensorHistory_ShouldAddAndReturnSensorHistory()
    {
        // Arrange
        var sensorHistory = new SensorHistory
        {
            SensorHistoryId = Guid.NewGuid(),
            DeviceId = Guid.NewGuid(),
            Temperature = 23.5,
            Humidity = 55.0,
            Time = DateTime.UtcNow
        };

        // Act
        var result = await _repository.AddSensorHistory(sensorHistory);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.SensorHistoryId, Is.EqualTo(sensorHistory.SensorHistoryId));

        var saved = await _context.SensorHistories
            .FirstOrDefaultAsync(sh => sh.SensorHistoryId == sensorHistory.SensorHistoryId);

        Assert.That(saved, Is.Not.Null, "SensorHistory was not saved to the database.");
        Assert.That(saved.Temperature, Is.EqualTo(23.5));
        Assert.That(saved.Humidity, Is.EqualTo(55.0));
    }
}