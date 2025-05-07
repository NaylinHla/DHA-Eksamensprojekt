using Core.Domain.Entities;
using Core.Domain.Exceptions;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Startup.Tests.GreenhouseDeviceTests
{
    public class GreenhouseDeviceRepositoryTests
    {
        private MyDbContext _context = null!;
        private GreenhouseDeviceRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
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
                Country = "Wonderland",
            };

            var deviceId = Guid.NewGuid();
            var userDevice = new UserDevice
            {
                DeviceId = deviceId,
                UserId = userId,
                DeviceName =    "Test",
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
            Assert.That(result.Email, Is.EqualTo("test@example.com"));
        }
        
        [Test]
        public void GetRecentSensorHistory_ShouldReturnOrderedList_ByDescendingTime()
        {
            // Arrange
            var now = DateTime.UtcNow;

            var deviceId = Guid.NewGuid();

            var sensorData = new List<SensorHistory>
            {
                new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 20, Humidity = 40, Time = now.AddMinutes(-10) },
                new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 22, Humidity = 45, Time = now },
                new() { SensorHistoryId = Guid.NewGuid(), DeviceId = deviceId, Temperature = 21, Humidity = 42, Time = now.AddMinutes(-5) },
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
        
    }
}
