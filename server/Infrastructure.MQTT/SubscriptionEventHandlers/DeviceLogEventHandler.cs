using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.Json;
using Application;
using Application.Interfaces;
using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;

namespace Infrastructure.MQTT.SubscriptionEventHandlers;

public class DeviceLogEventHandler(IGreenhouseDeviceService greenhouseDeviceService) : IMqttMessageHandler
{
    public string TopicFilter { get; } = StringConstants.Device + "/+/" + StringConstants.SensorData;
    public QualityOfService QoS { get; } = QualityOfService.AtLeastOnceDelivery;

    public void Handle(object? sender, OnMessageReceivedEventArgs args)
    {
        var dto = JsonSerializer.Deserialize<DeviceSensorDataDto>(args.PublishMessage.PayloadAsString,
                      JsonDefaults.CaseInsensitive)
                  ?? throw new SerializationException("Could not deserialize into " + nameof(DeviceSensorDataDto) +
                                                      " from " +
                                                      args.PublishMessage.PayloadAsString);
        var context = new ValidationContext(dto);
        Validator.ValidateObject(dto, context);
        greenhouseDeviceService.AddToDbAndBroadcast(dto);
    }
}