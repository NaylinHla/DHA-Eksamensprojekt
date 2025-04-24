using Application.Interfaces.Infrastructure.MQTT;
using Microsoft.Extensions.Logging;

namespace Application.Interfaces;

public class MockMqttPublisher(ILogger<MockMqttPublisher> logger) : IMqttPublisher
{
    public Task Publish(object dto, string topic)
    {
        logger.LogWarning($"Mock publish to topic {topic}: {dto}");
        return Task.CompletedTask;
    }
}