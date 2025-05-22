using System.Reflection;
using Core.Domain.Entities;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Exceptions;

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

        // GetConditionAlertUserDeviceIdByConditionAlertIdAsync

        [Test]
        public async Task GetConditionAlertUserDeviceIdByConditionAlertIdAsync_ShouldRespectIsDeletedFilter()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var otherDeviceId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();

            var userDevice = new UserDevice
            {
                UserId = userId,
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "100"
            };

            var otherUserDevice = new UserDevice
            {
                UserId = otherUserId,
                DeviceId = otherDeviceId,
                DeviceName = "Other",
                DeviceDescription = "Other device",
                WaitTime = "100"
            };

            var notDeletedAlert = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDevice = userDevice,
                UserDeviceId = deviceId,
                IsDeleted = false,
                SensorType = "Temperature",
                Condition = "<=30"
            };

            var deletedAlert = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDevice = userDevice,
                UserDeviceId = deviceId,
                IsDeleted = true,
                SensorType = "Humidity",
                Condition = ">=50"
            };

            var wrongDeviceAlert = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDevice = otherUserDevice,
                UserDeviceId = otherDeviceId,
                IsDeleted = false,
                SensorType = "Pressure",
                Condition = ">10"
            };

            _context.UserDevices.AddRange(userDevice, otherUserDevice);
            _context.ConditionAlertUserDevice.AddRange(notDeletedAlert, deletedAlert, wrongDeviceAlert);
            await _context.SaveChangesAsync();

            // Act: fetch by single-alert methods
            var resultNotDeleted = await _repository.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(
                notDeletedAlert.ConditionAlertUserDeviceId);
            var resultDeleted = await _repository.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(
                deletedAlert.ConditionAlertUserDeviceId);

            // Act: fetch all alerts for the user (matches method signature & log)
            var allForUser = await _repository.GetAllConditionAlertUserDevicesAsync(userId);

            // Assert: single-alert lookups
            Assert.That(resultNotDeleted, Is.Not.Null, "Not-deleted alert should be found");
            using (Assert.EnterMultipleScope())
            {
                Assert.That(resultNotDeleted!.ConditionAlertUserDeviceId,
                    Is.EqualTo(notDeletedAlert.ConditionAlertUserDeviceId));

                Assert.That(resultDeleted, Is.Null, "Deleted alert should NOT be found");
            }

            // (Optional) Verify data in the context directly
            var directQuery = await _context.ConditionAlertUserDevice
                .Where(c => c.UserDeviceId == deviceId && !c.IsDeleted)
                .ToListAsync();
            using (Assert.EnterMultipleScope())
            {
                Assert.That(directQuery, Has.Exactly(1).Items,
                    "Direct query should find exactly one non-deleted alert");
                Assert.That(allForUser, Has.Exactly(1).Items,
                    "GetAllConditionAlertUserDevicesAsync should return exactly one alert");
            }

            Assert.That(allForUser[0].ConditionAlertUserDeviceId,
                Is.EqualTo(notDeletedAlert.ConditionAlertUserDeviceId));
        }

        // GetAllConditionAlertPlantForAllUserAsync

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
        public async Task GetAllConditionAlertPlantForAllUserAsync_MultipleRecords_ReturnsOnlyNonDeleted()
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
                IsDeleted = false
            };

            var condition3 = new ConditionAlertPlant
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = true,
                IsDeleted = true
            };

            _context.ConditionAlertPlant.AddRange(condition1, condition2, condition3);
            await _context.SaveChangesAsync();

            var result = await _repository.GetAllConditionAlertPlantForAllUserAsync();

            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result.Any(r => r.ConditionAlertPlantId == condition3.ConditionAlertPlantId), Is.False);
        }

        // DeleteConditionAlertPlantAsync

        [Test]
        public async Task DeleteConditionAlertPlantAsync_ShouldHandleAllBranches()
        {
            // 1) Missing ID -> NotFoundException
            var missingId = Guid.NewGuid();
            var notFoundEx = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _repository.DeleteConditionAlertPlantAsync(missingId));
            Assert.That(notFoundEx.Message, Is.EqualTo(AlertConditionNotFound));

            // 2) Seed a new, non-deleted plant condition
            var condition = new ConditionAlertPlant
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = false,
                IsDeleted = false
            };
            _context.ConditionAlertPlant.Add(condition);
            await _context.SaveChangesAsync();

            // 3) First delete -> should mark as deleted (happy path)
            await _repository.DeleteConditionAlertPlantAsync(condition.ConditionAlertPlantId);
            var reloaded = await _context.ConditionAlertPlant
                .FirstOrDefaultAsync(c => c.ConditionAlertPlantId == condition.ConditionAlertPlantId);
            Assert.That(reloaded, Is.Not.Null);
            Assert.That(reloaded!.IsDeleted, Is.True);

            // 4) Second delete on the same ID -> already deleted -> throws NotFoundException
            var deletedEx = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _repository.DeleteConditionAlertPlantAsync(condition.ConditionAlertPlantId));
            Assert.That(deletedEx.Message, Is.EqualTo(AlertConditionNotFound));
        }

        private const string AlertConditionNotFound = "Alert Condition not found.";

        // ConditionAlertExistsAsync

        [Test]
        public async Task ConditionAlertExistsAsync_ExcludeIdBehavior_WorksCorrectly()
        {
            // Arrange
            var devId = Guid.NewGuid();
            var alert1 = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = devId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            };
            var alert2 = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = devId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            };
            _context.ConditionAlertUserDevice.AddRange(alert1, alert2);
            await _context.SaveChangesAsync();

            var method = typeof(AlertConditionRepository)
                .GetMethod("ConditionAlertExistsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act 1: excludeId = null
            var task1 = (Task<bool>)method.Invoke(
                _repository,
                [devId, "Temperature", "<=25", null]
            )!;
            var existsNoExclude = await task1;

            // Act 2: excludeId = alert1.Id
            var task2 = (Task<bool>)method.Invoke(
                _repository,
                [devId, "Temperature", "<=25", alert1.ConditionAlertUserDeviceId]
            )!;
            var existsExcl1 = await task2;

            // Act 3: excludeId = alert2.Id
            var task3 = (Task<bool>)method.Invoke(
                _repository,
                [devId, "Temperature", "<=25", alert2.ConditionAlertUserDeviceId]
            )!;
            var existsExcl2 = await task3;

            // Act 4: excludeId = unknown GUID
            var task4 = (Task<bool>)method.Invoke(
                _repository,
                [devId, "Temperature", "<=25", Guid.NewGuid()]
            )!;
            var existsExclUnknown = await task4;

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(existsNoExclude, Is.True);
                Assert.That(existsExcl1, Is.True);
                Assert.That(existsExcl2, Is.True);
                Assert.That(existsExclUnknown, Is.True);
            });
        }

        [Test]
        public async Task ConditionAlertExistsAsync_ShouldReturnFalse_WhenOnlyMatchingAlertIsExcluded()
        {
            // Arrange
            var devId = Guid.NewGuid();
            var alert = new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = devId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            };

            // Act
            _context.ConditionAlertUserDevice.Add(alert);
            await _context.SaveChangesAsync();

            var method = typeof(AlertConditionRepository)
                .GetMethod("ConditionAlertExistsAsync", BindingFlags.NonPublic | BindingFlags.Instance)!;

            var task = (Task<bool>)method.Invoke(
                _repository,
                [devId, "Temperature", "<=25", alert.ConditionAlertUserDeviceId]
            )!;
            var result = await task;

            // Assert
            Assert.That(result, Is.False);
        }


        // IsAlertUserDeviceConditionMeet

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
            _context.ConditionAlertUserDevice.AddRange(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = condition1Id,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=30",
                IsDeleted = false
            }, new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = condition2Id,
                UserDeviceId = deviceId,
                SensorType = "Humidity",
                Condition = ">=50",
                IsDeleted = false
            }, new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = condition3Id,
                UserDeviceId = deviceId,
                SensorType = "AirPressure",
                Condition = ">=1000",
                IsDeleted = false
            }, new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = condition4Id,
                UserDeviceId = deviceId,
                SensorType = "AirQuality",
                Condition = "<=50",
                IsDeleted = false
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

        [TestCase("42")]
        [TestCase("abc")]
        [TestCase("<50")]
        [TestCase(">23.5")]
        [TestCase("")]
        public void TryParseCondition_ShouldReturnFalse_ForInvalidConditions(string input)
        {
            // Act
            var result = AlertConditionRepository.TryParseCondition(input, out var op, out var threshold);

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.False, $"Expected '{input}' to be rejected");
                Assert.That(string.IsNullOrEmpty(op), Is.True,
                    $"Expected operator to be null or empty for input '{input}'");
                Assert.That(threshold, Is.EqualTo(0), $"Expected threshold to be 0 for input '{input}'");
            });
        }

        [Test]
        public async Task CheckAlertConditions_ShouldReturnEmptyList_WhenDeviceNotFound()
        {
            // Arrange
            var nonExistentDeviceId = Guid.NewGuid().ToString();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = nonExistentDeviceId,
                Temperature = 25,
                Humidity = 50,
                AirPressure = 1000,
                AirQuality = 10
            };

            // Act
            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty, "Expected empty list when device does not exist");
        }

        private static IEnumerable<TestCaseData> ValidSensorValues =>
        [
            new("Temperature", new IsAlertUserDeviceConditionMeetDto { Temperature = 23.5f }, 23.5f),
            new("Humidity", new IsAlertUserDeviceConditionMeetDto { Humidity = 50.1f }, 50.1f),
            new("AirPressure", new IsAlertUserDeviceConditionMeetDto { AirPressure = 1013.25f }, 1013.25f),
            new("AirQuality", new IsAlertUserDeviceConditionMeetDto { AirQuality = 45 }, 45f)
        ];

        [TestCaseSource(nameof(ValidSensorValues))]
        public void GetSensorValue_ShouldReturnCorrectValue(string sensorType, IsAlertUserDeviceConditionMeetDto dto,
            float expected)
        {
            var result = AlertConditionRepository.GetSensorValue(sensorType, dto);
            Assert.That(result, Is.EqualTo(expected));
        }

        private static IEnumerable<TestCaseData> NullSensorValues =>
        [
            new("Temperature", new IsAlertUserDeviceConditionMeetDto { Temperature = null }),
            new("Humidity", new IsAlertUserDeviceConditionMeetDto { Humidity = null }),
            new("AirPressure", new IsAlertUserDeviceConditionMeetDto { AirPressure = null }),
            new("UnknownSensor", new IsAlertUserDeviceConditionMeetDto())
        ];

        [TestCaseSource(nameof(NullSensorValues))]
        public void GetSensorValue_ShouldReturnNull(string sensorType, IsAlertUserDeviceConditionMeetDto dto)
        {
            var result = AlertConditionRepository.GetSensorValue(sensorType, dto);
            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_SensorValueEqualsThreshold_ReturnsMatch()
        {
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();
            var conditionId2 = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "10"
            });

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            });

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId2,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = ">=25",
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
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(conditionId.ToString()));
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_DeletedOrWrongDeviceConditions_AreIgnored()
        {
            var deviceId = Guid.NewGuid();
            var otherDeviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Main Device",
                DeviceDescription = "Device for test",
                WaitTime = "100"
            });

            _context.ConditionAlertUserDevice.AddRange(new[]
            {
                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = Guid.NewGuid(),
                    UserDeviceId = otherDeviceId, // different device
                    SensorType = "Temperature",
                    Condition = "<=30",
                    IsDeleted = false
                },
                new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = Guid.NewGuid(),
                    UserDeviceId = deviceId,
                    SensorType = "Temperature",
                    Condition = "<=30",
                    IsDeleted = true // deleted
                }
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty, "Deleted or unrelated device conditions should be ignored");
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_AllConditionsFail_ReturnsEmptyList()
        {
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Device description",
                WaitTime = "100"
            });

            _context.ConditionAlertUserDevice.AddRange(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=10",
                IsDeleted = false
            }, new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = Guid.NewGuid(),
                UserDeviceId = deviceId,
                SensorType = "Humidity",
                Condition = ">=90",
                IsDeleted = false
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 25,
                Humidity = 50
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }


        [Test]
        public async Task AddAlert_ShouldSkipDuplicateAlert_WhenSameAlertAddedTwice()
        {
            // Arrange
            var deviceId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Test Device",
                DeviceDescription = "Test device description",
                WaitTime = "10"
            });

            var conditionId = Guid.NewGuid();

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            });

            await _context.SaveChangesAsync();

            var alertUserDto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 24.5f,
            };

            // Act
            var matchedConditions = await _repository.IsAlertUserDeviceConditionMeet(alertUserDto);

            Assert.That(matchedConditions, Does.Contain(conditionId.ToString()));

            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = "<=25",
                AlertTime = DateTime.UtcNow,
                AlertName = "Test Alert",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            await _repository.IsAlertUserDeviceConditionMeet(alertUserDto);

            var alertsInDb = _context.Alerts.Where(a => a.AlertDeviceConditionId == conditionId).ToList();

            // Assert
            Assert.That(alertsInDb, Has.Count.EqualTo(1), "Duplicate alerts should be skipped in DB");
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ShouldSkipConditionIfRecentDuplicateAlertWithinCutoff()
        {
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();
            var cutoff = DateTime.UtcNow.AddHours(-12);

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Test device",
                WaitTime = "10"
            });

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            });

            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = "24 Text <= 25",
                AlertTime = cutoff.AddMinutes(1),
                AlertName = "Test Alert",
                AlertUserId = Guid.NewGuid()
            });


            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 24
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Is.Empty, "Condition should be skipped because of recent duplicate alert");
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_ShouldIncludeConditionIfAlertOlderThanCutoff()
        {
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();
            var cutoff = DateTime.UtcNow.AddHours(-12);

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Test device",
                WaitTime = "10"
            });

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=25",
                IsDeleted = false
            });

            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = "<=25",
                AlertTime = cutoff.AddMinutes(-1), // older alert, outside cutoff window
                AlertName = "Test Alert",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 24
            };

            var result = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(result, Has.One.EqualTo(conditionId.ToString()),
                "Condition should be included because alert is older than cutoff");
        }

        [TestCase("25 <= 25", "<=", 25, 25, "AlertTimeEqualsCutoff", 0, false)]
        [TestCase("25 <= 25", "<=", 25, 25, "OlderThanCutoff", -1, false)]
        [TestCase("25 <= 25", "<=", 25, 25, "EmptyDesc", 6, true)]
        [TestCase("26 Message <= Message 25.0", "<=", 25, 26, "BadFormatWithDelta", 0, true)]
        [TestCase("24.5 <= 25", "<=", 25, 24.7, "NearDuplicate", 2, true)]
        [TestCase("invalid desc", "<=", 25, 24.7, "UnparsableDesc", 0, false)]
        [TestCase("", "<=", 25, 24.7, "EmptyDescription", 11, false)]
        [TestCase("24.5 <= 30", "<=", 25, 24.7, "ThresholdMismatch", 6, false)]
        [TestCase("24.5 <= 25", "<=", 25, 24.7, "ExactDuplicate", 6, true)]
        [TestCase("23.5 <= 25", "<=", 25, 24.5, "DeltaOne", 6, true)]
        [TestCase("23.5 <= 25", "<=", 25, 22.5, "DeltaMoreThanOne", 5, true)]
        public async Task IsAlertUserDeviceConditionMeet_HasRecentDuplicateAlert_Cases(
            string alertDesc,
            string currentOp,
            double currentThreshold,
            double currentReading,
            string scenario,
            int hoursOffsetFromCutoff,
            bool shouldSkip)
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();
            var cutoffTime = DateTime.UtcNow.AddHours(-12);

            var alertTime = cutoffTime.AddHours(hoursOffsetFromCutoff);

            Console.WriteLine(alertTime);

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "Device",
                DeviceDescription = "Test device",
                WaitTime = "10"
            });

            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = $"{currentOp}{currentThreshold}",
                IsDeleted = false
            });

            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = alertDesc,
                AlertTime = alertTime,
                AlertName = "Test Alert",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = currentReading
            };

            // Act
            var results = await _repository.IsAlertUserDeviceConditionMeet(dto);

            // Assert
            if (shouldSkip)
                Assert.That(results, Is.Empty, $"[{scenario}] Expected alert to be skipped due to recent duplicate");
            else
                Assert.That(results, Has.One.EqualTo(conditionId.ToString()),
                    $"[{scenario}] Expected alert to be included");
        }


        [Test]
        public async Task IsAlertUserDeviceConditionMeet_IgnoresAlertsWithNullConditionId()
        {
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                WaitTime = "5",
                DeviceName = "Test",
                DeviceDescription = "Test"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=100",
                IsDeleted = false
            });

            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = null,
                AlertDesc = "<=100",
                AlertTime = DateTime.UtcNow,
                AlertName = "Orphan",
                AlertUserId = Guid.NewGuid()
            });
            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = "<=100",
                AlertTime = DateTime.UtcNow,
                AlertName = "Real",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 50f
            };

            var matched = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(matched, Has.Count.EqualTo(1));
            Assert.That(matched.Single(), Is.EqualTo(conditionId.ToString()));
        }

        [Test]
        public async Task IsAlertUserDeviceConditionMeet_Includes_WhenAlertDescCannotBeParsedByRegex()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                WaitTime = "5",
                DeviceName = "Test",
                DeviceDescription = "Test"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = conditionId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=50",
                IsDeleted = false
            });

            // badly formatted desc (regex should fail)
            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = conditionId,
                AlertDesc = "nonsense payload",
                AlertTime = DateTime.UtcNow,
                AlertName = "BadDesc",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 40f
            };

            // Act
            var matched = await _repository.IsAlertUserDeviceConditionMeet(dto);

            Assert.That(matched, Has.One.EqualTo(conditionId.ToString()));
        }
        
        [Test]
        public async Task IsAlertUserDeviceConditionMeet_PicksMostRecentAlertForDuplicateLogic()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var condId = Guid.NewGuid();

            _context.UserDevices.Add(new UserDevice
            {
                DeviceId = deviceId,
                DeviceName = "D",
                DeviceDescription = "D",
                WaitTime = "5"
            });
            _context.ConditionAlertUserDevice.Add(new ConditionAlertUserDevice
            {
                ConditionAlertUserDeviceId = condId,
                UserDeviceId = deviceId,
                SensorType = "Temperature",
                Condition = "<=20",
                IsDeleted = false
            });

            // older alert has a tight-match reading=19
            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = condId,
                AlertDesc = "19 <= 20",
                AlertTime = DateTime.UtcNow.AddMinutes(-10),
                AlertName = "Old",
                AlertUserId = Guid.NewGuid()
            }); 
            _context.Alerts.Add(new Alert
            {
                AlertId = Guid.NewGuid(),
                AlertDeviceConditionId = condId,
                AlertDesc = "10 <= 20",
                AlertTime = DateTime.UtcNow,
                AlertName = "New",
                AlertUserId = Guid.NewGuid()
            });

            await _context.SaveChangesAsync();

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = deviceId.ToString(),
                Temperature = 19.5f
            };

            // Act
            var matched = await _repository.IsAlertUserDeviceConditionMeet(dto);

            // Assert:
            Assert.That(matched, Has.One.EqualTo(condId.ToString()));
        }
    }
}