using System.Text.Json;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Models;
using HiveMQtt.Client;
using HiveMQtt.Client.Exceptions;
using HiveMQtt.MQTT5.Types;
using Microsoft.Extensions.Options;

namespace Infrastructure.MQTT;

public static class MqttExtensions
{
    public static IServiceCollection RegisterMqttInfrastructure(this IServiceCollection services)
    {
        services.AddSingleton<HiveMQClient>(sp =>
        {
            var optionsMonitor = sp.GetRequiredService<IOptionsMonitor<AppOptions>>();
            var logger = sp.GetRequiredService<ILogger<HiveMQClient>>();

            var options = new HiveMQClientOptionsBuilder()
                .WithWebSocketServer(
                    $"wss://{optionsMonitor.CurrentValue.MQTT_BROKER_HOST}:8884/mqtt") // Using WSS (secure WebSocket)
                .WithClientId($"myClientId_{Guid.NewGuid()}")
                .WithCleanStart(true)
                .WithKeepAlive(30)
                .WithAutomaticReconnect(true)
                .WithMaximumPacketSize(1024)
                .WithReceiveMaximum(100)
                .WithSessionExpiryInterval(3600)
                .WithUserName(optionsMonitor.CurrentValue.MQTT_USERNAME)
                .WithPassword(optionsMonitor.CurrentValue.MQTT_PASSWORD)
                .WithRequestProblemInformation(true)
                .WithRequestResponseInformation(true)
                .WithAllowInvalidBrokerCertificates(true)
                .Build();

            var client = new HiveMQClient(options);

            client.OnDisconnectReceived += (sender, args) => { logger.LogWarning("MQTT client disconnected"); };

            client.OnMessageReceived += (eventObject, args) =>
            {
                logger.LogInformation("Global handler - Message Received: {sender}",
                    JsonSerializer.Serialize(eventObject));
                logger.LogInformation("Global handler - Message Received: {payload}",
                    args.PublishMessage.PayloadAsString);
            };

            const int maxRetries = 5;
            for (var attempt = 1; attempt <= maxRetries; attempt++)
                try
                {
                    logger.LogInformation("Attempting to connect to MQTT broker (attempt {attempt}/{maxRetries})",
                        attempt, maxRetries);

                    var connectResult = client.ConnectAsync().GetAwaiter().GetResult();

                    logger.LogInformation("Connection successful on attempt {attempt}. Result: {result}",
                        attempt,
                        JsonSerializer.Serialize(new
                        {
                            connectResult.ResponseInformation,
                            connectResult.ReasonString
                        }));

                    break;
                }
                catch (HiveMQttClientException ex)
                {
                    logger.LogError(ex, "Error connecting to MQTT broker on attempt {attempt}", attempt);

                    if (attempt == maxRetries)
                        logger.LogError("Max retries reached");
                    Thread.Sleep(TimeSpan.FromSeconds(Math.Pow(2, attempt)));
                }

            return client;
        });
        //here we're adding the handlers to the DI
        var subscribeHandlers = typeof(IMqttMessageHandler).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IMqttMessageHandler).IsAssignableFrom(t));
        foreach (var handlerType in subscribeHandlers) services.AddScoped(handlerType);

        services.AddSingleton<IMqttPublisher, MqttPublisher>();
        return services;
    }


    public static async Task<WebApplication> ConfigureMqtt(this WebApplication app)
    {
        var mqttClient = app.Services.GetRequiredService<HiveMQClient>();
        var builder = new SubscribeOptionsBuilder();
        var logger = app.Services.GetRequiredService<ILogger<HiveMQClient>>();

        var handlerTypes = typeof(IMqttMessageHandler).Assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && typeof(IMqttMessageHandler).IsAssignableFrom(t));

        //here we're subscribing to each handler
        foreach (var handlerType in handlerTypes)
            using (var scope = app.Services.CreateScope())
            {
                var handler = (IMqttMessageHandler)scope.ServiceProvider
                    .GetRequiredService(handlerType);

                logger.LogInformation("Subscribing to topic: {topic} with QoS: {qos}",
                    handler.TopicFilter, handler.QoS);

                builder.WithSubscription(
                    new TopicFilter(handler.TopicFilter, handler.QoS),
                    (sender, args) =>
                    {
                        using var messageScope = app.Services.CreateScope();
                        var messageHandler = (IMqttMessageHandler)messageScope.ServiceProvider
                            .GetRequiredService(handlerType);
                        messageHandler.Handle(sender, args);
                    });
            }

        await mqttClient.SubscribeAsync(builder.Build());
        return app;
    }
}