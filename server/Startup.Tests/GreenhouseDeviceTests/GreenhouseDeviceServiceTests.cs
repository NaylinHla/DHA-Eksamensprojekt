using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Application.Models.Dtos.RestDtos;
using Application.Services;
using Core.Domain.Entities;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.GreenhouseDeviceTests
{
    public class GreenhouseDeviceServiceTests
    {
        private GreenhouseDeviceService _service;
        private Mock<IGreenhouseDeviceRepository> _repositoryMock;
        private Mock<IConnectionManager> _connectionManagerMock;

        [SetUp]
        public void Setup()
        {
            // Mock the repository and connection manager
            _repositoryMock = new Mock<IGreenhouseDeviceRepository>();
            _connectionManagerMock = new Mock<IConnectionManager>();
            var alertServiceMock = new Mock<IAlertService>();
            
            alertServiceMock
                .Setup(a => a.TriggerUserDeviceConditionAsync(It.IsAny<IsAlertUserDeviceConditionMeetDto>()))
                .Returns(Task.CompletedTask);

            var services = new ServiceCollection();

            services.AddSingleton(_repositoryMock.Object);
            services.AddSingleton(alertServiceMock.Object);
            services.AddLogging();

            var serviceProvider = services.BuildServiceProvider();

            _service = new GreenhouseDeviceService(
                serviceProvider, 
                _connectionManagerMock.Object, 
                Mock.Of<IValidator<DeviceSensorDataDto>>());
        }

        [Test]
        public void AddToDbAndBroadcast_ShouldReturnEarly_WhenDtoIsNull()
        {
            // Act & Assert: no exception for null
            Assert.DoesNotThrowAsync(() => _service.AddToDbAndBroadcast(null));
        }

        [Test]
        public async Task AddToDbAndBroadcast_ShouldAddSensorHistory_WhenDtoIsValid()
        {
            // Arrange
            var dto = new DeviceSensorDataDto
            {
                DeviceId    = Guid.NewGuid().ToString(),
                Temperature = 22.5,
                Humidity    = 55,
                AirPressure = 1013.25,
                AirQuality  = 30,
                Time        = DateTime.UtcNow
            };

            // Mock AddSensorHistory to just return the entity
            _repositoryMock
                .Setup(r => r.AddSensorHistory(It.IsAny<SensorHistory>()))
                .ReturnsAsync((SensorHistory s) => s);

            // Also mock the subsequent GetSensorHistory to return one record matching our dto
            _repositoryMock
                .Setup(r => r.GetSensorHistoryByDeviceIdAsync(Guid.Parse(dto.DeviceId), null, null))
                .ReturnsAsync([
                    new GetAllSensorHistoryByDeviceIdDto
                    {
                        DeviceId = Guid.Parse(dto.DeviceId),
                        DeviceName = "MockDevice",
                        SensorHistoryRecords =
                        [
                            new SensorHistoryDto
                            {
                                Temperature = dto.Temperature,
                                Humidity = dto.Humidity,
                                AirPressure = dto.AirPressure,
                                AirQuality = dto.AirQuality,
                                Time = dto.Time
                            }
                        ]
                    }
                ]);

            // Act
            await _service.AddToDbAndBroadcast(dto);

            // Assert that we saved exactly the right SensorHistory
            _repositoryMock.Verify(r => r.AddSensorHistory(It.Is<SensorHistory>(s =>
                s.DeviceId   == Guid.Parse(dto.DeviceId) &&
                Math.Abs(s.Temperature - dto.Temperature) < 0.001 &&
                Math.Abs(s.Humidity    - dto.Humidity)    < 0.001 &&
                Math.Abs(s.AirPressure - dto.AirPressure) < 0.001 &&
                s.AirQuality == dto.AirQuality &&
                s.Time       == dto.Time
            )), Times.Once);

            // And that we broadcast the exact same data
            _connectionManagerMock.Verify(cm => cm.BroadcastToTopic(
                $"GreenhouseSensorData/{dto.DeviceId}",
                It.Is<ServerBroadcastsLiveDataToDashboard>(msg =>
                    msg.Logs.Count == 1 &&
                    msg.Logs[0].DeviceId == Guid.Parse(dto.DeviceId) &&
                    msg.Logs[0].SensorHistoryRecords.Count == 1 &&
                    Math.Abs(msg.Logs[0].SensorHistoryRecords[0].Temperature - dto.Temperature) < 0.001
                )), Times.Once);
        }

        [Test]
        public async Task AddToDbAndBroadcast_ShouldBroadcastWithRecentHistory()
        {
            // Arrange
            var deviceId = Guid.NewGuid();
            var dto = new DeviceSensorDataDto
            {
                DeviceId    = deviceId.ToString(),
                Temperature = 22,
                Humidity    = 60,
                AirPressure = 1012,
                AirQuality  = 85,
                Time        = DateTime.UtcNow
            };

            var mockHistory = new List<GetAllSensorHistoryByDeviceIdDto>
            {
                new()
                {
                    DeviceId = deviceId,
                    DeviceName = "MockDevice",
                    SensorHistoryRecords =
                    [
                        new SensorHistoryDto
                        {
                            Temperature = 22,
                            Humidity = 60,
                            AirPressure = 1012,
                            AirQuality = 85,
                            Time = dto.Time
                        }
                    ]
                }
            };

            _repositoryMock
                .Setup(r => r.GetSensorHistoryByDeviceIdAsync(deviceId, null, null))
                .ReturnsAsync(mockHistory);

            // Act
            await _service.AddToDbAndBroadcast(dto);

            // Assert we broadcast that mockHistory back out, with a tolerance on the temperature
            _connectionManagerMock.Verify(cm => cm.BroadcastToTopic(
                $"GreenhouseSensorData/{dto.DeviceId}",
                It.Is<ServerBroadcastsLiveDataToDashboard>(msg =>
                    msg.Logs.Count == 1 &&
                    msg.Logs[0].DeviceId == deviceId &&
                    msg.Logs[0].SensorHistoryRecords.Count == 1 &&
                    // Use a small epsilon rather than exact equality:
                    Math.Abs(msg.Logs[0].SensorHistoryRecords[0].Temperature - 22) < 0.001
                )), Times.Once);

        }
    }
}