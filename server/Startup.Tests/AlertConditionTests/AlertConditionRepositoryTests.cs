using Core.Domain.Entities;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Application.Models.Dtos.RestDtos;

namespace Startup.Tests.AlertConditionTests
{
    public class AlertConditionRepositoryTests
    {
        private MyDbContext _context = null!;
        private AlertConditionRepository _repository = null!;

        [SetUp]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<MyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new MyDbContext(options);
            _repository = new AlertConditionRepository(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }


        [Test]
        public async Task GetAllConditionAlertPlantForAllUserAsync_NoRecords_ReturnsEmptyList()
        {
            var result = await _repository.GetAllConditionAlertPlantForAllUserAsync();
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllConditionAlertPlantForAllUserAsync_DeletedRecords_AreExcluded()
        {
            var condition1 = new ConditionAlertPlant
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = true,
                IsDeleted = false
            };

            var condition2 = new ConditionAlertPlant
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = false,
                IsDeleted = true // should be excluded
            };

            _context.ConditionAlertPlant.AddRange(condition1, condition2);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllConditionAlertPlantForAllUserAsync();

            Assert.That(result, Has.Count.EqualTo(1));
            var first = result.First();

            using (Assert.EnterMultipleScope())
            {
                Assert.That(first.ConditionAlertPlantId, Is.EqualTo(condition1.ConditionAlertPlantId));
                Assert.That(first.PlantId, Is.EqualTo(condition1.PlantId));
                Assert.That(first.WaterNotify, Is.EqualTo(condition1.WaterNotify));
            }
        }

        [Test]
        public async Task GetAllConditionAlertPlantForAllUserAsync_ReturnsCorrectProjection()
        {
            var condition = new ConditionAlertPlant
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = true,
                IsDeleted = false
            };

            _context.ConditionAlertPlant.Add(condition);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllConditionAlertPlantForAllUserAsync();

            Assert.That(result, Has.Count.EqualTo(1));
            var dto = result[0];
            using (Assert.EnterMultipleScope())
            {
                Assert.That(dto.ConditionAlertPlantId, Is.EqualTo(condition.ConditionAlertPlantId));
                Assert.That(dto.PlantId, Is.EqualTo(condition.PlantId));
                Assert.That(dto.WaterNotify, Is.EqualTo(condition.WaterNotify));
            }
        }

        [Test]
        public void IsAlertUserDeviceConditionMeet_NullDto_ThrowsArgumentNullException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _repository.IsAlertUserDeviceConditionMeet(null!);
            });
        }

        [Test]
        public void IsAlertUserDeviceConditionMeet_EmptyUserDeviceId_ThrowsArgumentException_WithExpectedMessage()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto { UserDeviceId = null! };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _repository.IsAlertUserDeviceConditionMeet(dto);
            });

            Assert.That(ex.Message, Is.EqualTo("UserDeviceId is required."));
        }

        [Test]
        public void
            IsAlertUserDeviceConditionMeet_InvalidUserDeviceIdFormat_ThrowsArgumentException_WithExpectedMessage()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto { UserDeviceId = "not-a-guid" };

            var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _repository.IsAlertUserDeviceConditionMeet(dto);
            });

            Assert.That(ex.Message, Is.EqualTo("UserDeviceId must be a valid GUID."));
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_DeviceNotFound_ReturnsEmptyList()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = Guid.NewGuid().ToString()
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_NoActiveConditions_ReturnsEmptyList()
        {
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Test Device",
                DeviceDescription = "Test device description",
                WaitTime = "100"
            });
            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString()
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ConditionWithUnsupportedSensorType_IsSkipped()
        {
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "1000"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = deviceId,
                SensorType = "UnsupportedSensor",
                Condition = "<=50",
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty); // Unsupported sensor type means no matches
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ConditionWithInvalidConditionFormat_IsSkipped()
        {
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "100"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "INVALID_CONDITION",
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty); // Invalid condition format means no matches
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ConditionWithInvalidThreshold_IsSkipped()
        {
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "100"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=NotANumber",
                IsDeleted = false
            });
            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty); // Invalid threshold means no matches
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ValidConditions_ReturnsMatchedConditionIds()
        {
            var deviceId = Guid.NewGuid();
            var condition1Id = Guid.NewGuid();
            var condition2Id = Guid.NewGuid();
            var condition3Id = Guid.NewGuid();
            var condition4Id = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "10"
            });
            _context.ConditionAlertUserDevice.AddRange(new[]
            {
                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = condition1Id,
                    UserDeviceId = deviceId,
                    SensorType = "Temperature",
                    Condition = "<=30",
                    IsDeleted = false
                },
                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = condition2Id,
                    UserDeviceId = deviceId,
                    SensorType = "Humidity",
                    Condition = "=>50",
                    IsDeleted = false
                },

                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = condition3Id,
                    UserDeviceId = deviceId,
                    SensorType = "AirPressure",
                    Condition = ">=1000",
                    IsDeleted = false
                },
                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = condition4Id,
                    UserDeviceId = deviceId,
                    SensorType = "AirQuality",
                    Condition = "<=50",
                    IsDeleted = false
                }
            });
            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25,
                Humidity = 55
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(condition1Id.ToString()));
            Assert.That(result, Does.Contain(condition2Id.ToString()));
            Assert.That(result, Has.Count.EqualTo(2));
        }
    }
}