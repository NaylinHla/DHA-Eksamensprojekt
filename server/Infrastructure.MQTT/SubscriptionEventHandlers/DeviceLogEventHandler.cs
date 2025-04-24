using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Application.Interfaces;
using Application.Models;
using Application.Models.Dtos.MqttSubscriptionDto;
using HiveMQtt.Client.Events;
using HiveMQtt.MQTT5.Types;

namespace Infrastructure.MQTT.SubscriptionEventHandlers;

public class DeviceLogEventHandler(IWeatherStationService weatherStationService) : IMqttMessageHandler
{
    public string TopicFilter { get; } = StringConstants.Device + "/+/" + StringConstants.Log;
    public QualityOfService QoS { get; } = QualityOfService.AtLeastOnceDelivery;

    public void Handle(object? sender, OnMessageReceivedEventArgs args)
    {
        var dto = JsonSerializer.Deserialize<DeviceLogDto>(args.PublishMessage.PayloadAsString,
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? throw new Exception("Could not deserialize into " + nameof(DeviceLogDto) + " from " +
                                      args.PublishMessage.PayloadAsString);
        var context = new ValidationContext(dto);
        Validator.ValidateObject(dto, context);
        weatherStationService.AddToDbAndBroadcast(dto);
    }
}