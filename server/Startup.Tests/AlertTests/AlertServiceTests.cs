using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Application.Services;
using Core.Domain.Entities;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.AlertTests
{
    public class AlertServiceTests
    {
        private Mock<IAlertRepository> _alertRepoMock = null!;
        private Mock<IAlertConditionRepository> _condRepoMock = null!;
        private Mock<IPlantRepository> _plantRepoMock = null!;
        private Mock<IUserDeviceRepository> _deviceRepoMock = null!;
        private Mock<IConnectionManager> _wsMock = null!;
        private IAlertService _service = null!;

        private Guid _deviceId;
        private Guid _conditionId;

        [SetUp]
        public void SetUp()
        {
            _alertRepoMock = new Mock<IAlertRepository>();
            _condRepoMock = new Mock<IAlertConditionRepository>();
            _plantRepoMock = new Mock<IPlantRepository>();
            _deviceRepoMock = new Mock<IUserDeviceRepository>();
            _wsMock = new Mock<IConnectionManager>();

            _service = new AlertService(
                _alertRepoMock.Object,
                _condRepoMock.Object,
                _plantRepoMock.Object,
                _deviceRepoMock.Object,
                _wsMock.Object
            );

            _deviceId = Guid.NewGuid();
            _conditionId = Guid.NewGuid();
        }

        private UserDevice MakeMockUserDevice(Guid? userId = null) => new()
        {
            DeviceId = _deviceId,
            UserId = userId ?? Guid.NewGuid(),
            DeviceName = "Mock Device",
            DeviceDescription = "A device used for testing.",
            WaitTime = "10s"
        };

        [Test]
        public async Task NoMatch_DoesNothing()
        {
            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(It.IsAny<IsAlertUserDeviceConditionMeetDto>()))
                .ReturnsAsync([]);

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = _deviceId.ToString(),
                Temperature = 25
            };

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
            _wsMock.Verify(x => x.BroadcastToTopic(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Test]
        public async Task InvalidDeviceId_DoesNothing()
        {
            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(It.IsAny<IsAlertUserDeviceConditionMeetDto>()))
                .ReturnsAsync([_conditionId.ToString()]);

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = "not-a-guid",
                Temperature = 25
            };

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Test]
        public async Task MissingDevice_DoesNothing()
        {
            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(It.IsAny<IsAlertUserDeviceConditionMeetDto>()))
                .ReturnsAsync([_conditionId.ToString()]);

            _deviceRepoMock
                .Setup(x => x.GetUserDeviceByIdAsync(_deviceId))
                .ReturnsAsync((UserDevice?)null);

            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = _deviceId.ToString(),
                Temperature = 25
            };

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Test]
        public async Task InvalidConditionId_SkipsThatCondition()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = _deviceId.ToString(),
                Temperature = 25
            };

            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(dto))
                .ReturnsAsync(["not-a-guid"]);

            _deviceRepoMock
                .Setup(x => x.GetUserDeviceByIdAsync(_deviceId))
                .ReturnsAsync(MakeMockUserDevice());

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [Test]
        public async Task MissingSensorValue_SkipsCondition()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = _deviceId.ToString(),
                Temperature = null // nullable double
            };

            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(dto))
                .ReturnsAsync([_conditionId.ToString()]);

            _deviceRepoMock
                .Setup(x => x.GetUserDeviceByIdAsync(_deviceId))
                .ReturnsAsync(MakeMockUserDevice());

            _condRepoMock
                .Setup(x => x.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(_conditionId))
                .ReturnsAsync(new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = _conditionId,
                    SensorType = "Temperature",
                    Condition = "<=25",
                    UserDeviceId = _deviceId,
                });

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
        }


        [Test]
        public async Task MissingCondition_SkipsThatCondition()
        {
            var dto = new IsAlertUserDeviceConditionMeetDto
            {
                UserDeviceId = _deviceId.ToString(),
                Temperature = 25
            };

            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(dto))
                .ReturnsAsync([_conditionId.ToString()]);

            _deviceRepoMock
                .Setup(x => x.GetUserDeviceByIdAsync(_deviceId))
                .ReturnsAsync(MakeMockUserDevice());

            _condRepoMock
                .Setup(x => x.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(_conditionId))
                .ReturnsAsync((ConditionAlertUserDevice?)null);

            await _service.TriggerUserDeviceConditionAsync(dto);

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
        }

        [TestCase(25.43, "Temperature", "°C")]
        [TestCase(55.54, "Humidity", "%")]
        [TestCase(1013.43, "AirPressure", "hPa")]
        [TestCase(300, "AirQuality", "ppm")]
        public async Task ValidMatch_CreatesAndBroadcastsAlert_ForVariousSensors(double sensorValue, string sensorType,
            string unit)
        {
            IsAlertUserDeviceConditionMeetDto CreateDtoWithSensor(string type, double value)
            {
                return type switch
                {
                    "Temperature" => new IsAlertUserDeviceConditionMeetDto
                    {
                        UserDeviceId = _deviceId.ToString(),
                        Temperature = value
                    },
                    "Humidity" => new IsAlertUserDeviceConditionMeetDto
                    {
                        UserDeviceId = _deviceId.ToString(),
                        Humidity = value
                    },
                    "AirPressure" => new IsAlertUserDeviceConditionMeetDto
                    {
                        UserDeviceId = _deviceId.ToString(),
                        AirPressure = value
                    },
                    "AirQuality" => new IsAlertUserDeviceConditionMeetDto
                    {
                        UserDeviceId = _deviceId.ToString(),
                        AirQuality = (int)value
                    },
                    _ => throw new ArgumentException($"Unknown sensor type {type}")
                };
            }

            var userId = Guid.NewGuid();

            var dto = CreateDtoWithSensor(sensorType, sensorValue);

            _condRepoMock
                .Setup(x => x.IsAlertUserDeviceConditionMeet(dto))
                .ReturnsAsync([_conditionId.ToString()]);

            _deviceRepoMock
                .Setup(x => x.GetUserDeviceByIdAsync(_deviceId))
                .ReturnsAsync(MakeMockUserDevice(userId));

            _condRepoMock
                .Setup(x => x.GetConditionAlertUserDeviceIdByConditionAlertIdAsync(_conditionId))
                .ReturnsAsync(new ConditionAlertUserDevice
                {
                    ConditionAlertUserDeviceId = _conditionId,
                    UserDeviceId = _deviceId,
                    SensorType = sensorType,
                    Condition = "<=30"
                });

            Alert? savedAlert = null;
            _alertRepoMock
                .Setup(x => x.AddAlertAsync(It.IsAny<Alert>()))
                .Callback<Alert>(a => savedAlert = a)
                .ReturnsAsync(() => savedAlert!);

            await _service.TriggerUserDeviceConditionAsync(dto);

            Assert.That(savedAlert, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(savedAlert!.AlertDesc, Does.Contain($"{sensorValue}{unit}"));
                Assert.That(savedAlert.AlertDesc, Does.Contain("<=30"));
                Assert.That(savedAlert.AlertName, Does.Contain(sensorType));
                Assert.That(savedAlert.AlertUserId, Is.EqualTo(userId));
            }

            _wsMock.Verify(x => x.BroadcastToTopic(
                $"alerts-{userId}",
                It.Is<ServerBroadcastsLiveAlertToAlertView>(broadcast =>
                    broadcast.Alerts.Count == 1 &&
                    broadcast.Alerts[0].AlertDesc == savedAlert.AlertDesc
                )), Times.Once);
        }


        [Test]
        public async Task
            CheckAndTriggerScheduledPlantAlertsAsync_WhenWaterNotifyIsTrueAndPlantNeedsWater_CreatesAndBroadcastsAlert()
        {
            // Arrange
            var plantId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var conditionId = Guid.NewGuid();

            var plant = new Plant
            {
                PlantId = plantId,
                PlantName = "Aloe Vera",
                PlantType = "Succulent",
                UserPlants = new List<UserPlant> { new() { UserId = userId, PlantId = plantId } },
                LastWatered = DateTime.UtcNow.AddDays(-5),
                WaterEvery = 3,
                IsDead = false
            };

            var conditionAlert = new ConditionAlertPlantResponseDto
            {
                ConditionAlertPlantId = conditionId,
                PlantId = plantId,
                WaterNotify = true, // This flag might not be used in the service method, but keep for realism
            };

            // Setup alertConditionRepo mock to return the condition alert list (drive loop)
            _condRepoMock
                .Setup(x => x.GetAllConditionAlertPlantForAllUserAsync())
                .ReturnsAsync([conditionAlert]);

            // Setup plantRepo to return the plant when requested by PlantId
            _plantRepoMock
                .Setup(x => x.GetPlantByIdAsync(plantId))
                .ReturnsAsync(plant);

            // Setup plantRepo to return the userId of the plant owner
            _plantRepoMock
                .Setup(x => x.GetPlantOwnerUserId(plantId))
                .ReturnsAsync(userId);

            // Capture the alert passed to AddAlertAsync for assertions
            Alert? savedAlert = null;
            _alertRepoMock
                .Setup(x => x.AddAlertAsync(It.IsAny<Alert>()))
                .Callback<Alert>(a => savedAlert = a)
                .ReturnsAsync(() => savedAlert!);

            // Act
            await _service.CheckAndTriggerScheduledPlantAlertsAsync();

            // Assert - validate alert was created with expected properties
            Assert.That(savedAlert, Is.Not.Null);
            using (Assert.EnterMultipleScope())
            {
                Assert.That(savedAlert!.AlertUserId, Is.EqualTo(userId));
                Assert.That(savedAlert.AlertPlantConditionId, Is.EqualTo(conditionId));
                Assert.That(savedAlert.AlertName, Does.Contain("Scheduled Water Alert"));
                Assert.That(savedAlert.AlertDesc, Does.Contain("Aloe Vera"));
            }

            // Verify broadcast was sent to the correct user topic with expected alert
            _wsMock.Verify(x => x.BroadcastToTopic(
                $"alerts-{userId}",
                It.Is<ServerBroadcastsLiveAlertToAlertView>(broadcast =>
                    broadcast.Alerts.Count == 1 &&
                    broadcast.Alerts[0].AlertDesc == savedAlert.AlertDesc
                )), Times.Once);
        }


        [Test]
        public async Task CheckAndTriggerScheduledPlantAlertsAsync_WhenUserIdIsEmpty_SkipsAlertCreation()
        {
            var conditionAlert = new ConditionAlertPlantResponseDto
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = true
            };

            _condRepoMock
                .Setup(x => x.GetAllConditionAlertPlantForAllUserAsync())
                .ReturnsAsync([conditionAlert]);

            var plant = new Plant
            {
                PlantId = conditionAlert.PlantId,
                PlantName = "Tall Grass",
                UserPlants = new List<UserPlant>()
            };

            _plantRepoMock
                .Setup(x => x.GetPlantByIdAsync(conditionAlert.PlantId))
                .ReturnsAsync(plant);

            // Return Guid.Empty userId -> triggers skip
            _plantRepoMock
                .Setup(x => x.GetPlantOwnerUserId(conditionAlert.PlantId))
                .ReturnsAsync(Guid.Empty);

            // Act
            await _service.CheckAndTriggerScheduledPlantAlertsAsync();

            // Assert no alert created or broadcast
            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
            _wsMock.Verify(x => x.BroadcastToTopic(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }


        [Test]
        public async Task CheckAndTriggerScheduledPlantAlertsAsync_WhenPlantNotFound_SkipsAlertCreation()
        {
            var conditionAlert = new ConditionAlertPlantResponseDto
            {
                ConditionAlertPlantId = Guid.NewGuid(),
                PlantId = Guid.NewGuid(),
                WaterNotify = true
            };

            _condRepoMock
                .Setup(x => x.GetAllConditionAlertPlantForAllUserAsync())
                .ReturnsAsync([conditionAlert]);

            _plantRepoMock
                .Setup(x => x.GetPlantByIdAsync(conditionAlert.PlantId))
                .ReturnsAsync((Plant)null!);

            await _service.CheckAndTriggerScheduledPlantAlertsAsync();

            _alertRepoMock.Verify(x => x.AddAlertAsync(It.IsAny<Alert>()), Times.Never);
            _wsMock.Verify(x => x.BroadcastToTopic(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }
    }
}