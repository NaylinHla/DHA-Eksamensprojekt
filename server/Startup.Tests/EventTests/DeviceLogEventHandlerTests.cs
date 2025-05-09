using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Json;
using Application.Interfaces;
using Application.Models.Dtos.MqttSubscriptionDto;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;
using Infrastructure.MQTT.SubscriptionEventHandlers;
using Moq;
using NUnit.Framework;

namespace Startup.Tests.EventTests;

[TestFixture]
public class DeviceLogEventHandlerTests
{
    [SetUp]
    public void SetUp()
    {
        _mockGreenhouseService = new Mock<IGreenhouseDeviceService>();
        _handler = new DeviceLogEventHandler(_mockGreenhouseService.Object);
    }

    private Mock<IGreenhouseDeviceService> _mockGreenhouseService;
    private DeviceLogEventHandler _handler;

    [Test]
    public void Handle_ValidPayload_CallsAddToDbAndBroadcast()
    {
        // Arrange
        var dto = new DeviceSensorDataDto
        {
            DeviceId = "device-123",
            Time = DateTime.UtcNow,
            Temperature = 25,
            Humidity = 40
        };

        var payloadJson = JsonSerializer.Serialize(dto);
        var args = new OnMessageReceivedEventArgs(
            new MQTT5PublishMessage
            {
                Payload = Encoding.UTF8.GetBytes(payloadJson)
            }
        );


        // Act
        _handler.Handle(null, args);

        // Assert
        _mockGreenhouseService.Verify(s =>
            s.AddToDbAndBroadcast(It.Is<DeviceSensorDataDto>(d => d.DeviceId == dto.DeviceId)), Times.Once);
    }

    [Test]
    public void Handle_InvalidJson_ThrowsSerializationException()
    {
        var badJson = "{ not valid json ";
        var args = new OnMessageReceivedEventArgs(
            new MQTT5PublishMessage
            {
                Payload = Encoding.UTF8.GetBytes(badJson)
            }
        );

        Assert.Throws<JsonException>(() => _handler.Handle(null, args));
    }

    [Test]
    public void Handle_InvalidDto_ThrowsValidationException()
    {
        var invalidDto = new DeviceSensorDataDto
        {
            Time = DateTime.UtcNow,
            Temperature = 22,
            DeviceId = "" // required, must be set
        };

        var json = JsonSerializer.Serialize(invalidDto);
        var args = new OnMessageReceivedEventArgs(
            new MQTT5PublishMessage
            {
                Payload = Encoding.UTF8.GetBytes(json)
            });

        Assert.Throws<ValidationException>(() => _handler.Handle(null, args));
    }

    [Test]
    public void TopicFilter_IsCorrect()
    {
        Assert.That(_handler.TopicFilter, Is.EqualTo("Device/+/SensorData"));
    }

    [Test]
    public void QoS_IsAtLeastOnce()
    {
        Assert.That(_handler.QoS, Is.EqualTo(QualityOfService.AtLeastOnceDelivery));
    }
}