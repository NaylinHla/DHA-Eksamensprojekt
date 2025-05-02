using System.Collections.Concurrent;
using System.Text.Encodings.Web;
using System.Text.Json;
using Application;
using HiveMQtt.Client;

namespace Startup.Tests;

public class TestMqttClient
{
    public TestMqttClient(string host, string username)
    {
        var options = new HiveMQClientOptionsBuilder()
            .WithWebSocketServer(
                $"wss://{host}:443") // Using WSS (secure WebSocket)
            .WithClientId($"myClientId_{Guid.NewGuid()}")
            .WithCleanStart(true)
            .WithKeepAlive(30)
            .WithAutomaticReconnect(true)
            .WithMaximumPacketSize(1024)
            .WithReceiveMaximum(100)
            .WithSessionExpiryInterval(3600)
            .WithUserName(username)
            .WithRequestProblemInformation(true)
            .WithRequestResponseInformation(true)
            .WithAllowInvalidBrokerCertificates(true)
            .Build();
        MqttClient = new HiveMQClient(options);
        MqttClient.OnMessageReceived += (_, args) =>
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(args.PublishMessage.PayloadAsString);
            var stringRepresentation = JsonSerializer.Serialize(jsonElement, JsonDefaults.MqttSerialize);
            ReceivedMessages.Enqueue(stringRepresentation);
            Console.WriteLine($"Received message: {stringRepresentation}");
        };
        MqttClient.ConnectAsync().GetAwaiter().GetResult();
    }

    public string DeviceId { get; } = Guid.NewGuid().ToString();
    public HiveMQClient MqttClient { get; }
    public ConcurrentQueue<string> ReceivedMessages { get; } = new();
}