using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.MqttDtos.Response;
using Application.Models.Dtos.MqttSubscriptionDto;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;
using WebSocketBoilerplate;

namespace Startup.Tests.EventTests;

[TestFixture]
public class MqttTriggeredTests
{
    [SetUp]
    public void Setup()
    {
        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig();
                    services.DefaultTestConfig(makeMqttClient: true);
                });
            });

        _httpClient = factory.CreateClient();
        _scopedServiceProvider = factory.Services.CreateScope().ServiceProvider; // Create scope here for services
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [Test]
    public async Task WhenServerReceivesTimeSeriesData_ServerSavesInDbAndBroadcastsToClient()
    {
        // Arrange
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        var wsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();

        // Create a new scope to get access to DB context within the test
        using var scope = _scopedServiceProvider.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();

        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        // Get the first device's ID from the user
        var deviceId = user.UserDevices.FirstOrDefault()?.DeviceId ?? Guid.NewGuid();

        // Set up WebSocket to listen to the specific device topic
        await connectionManager.AddToTopic(StringConstants.GreenhouseSensorData + "/" + deviceId,
            wsClient.WsClientId);

        // Sample sensor reading to be sent
        var sensorReading = new SensorHistoryDto
        {
            Temperature = 21.3,
            Humidity = 45.2,
            AirPressure = 1009.6,
            AirQuality = 2,
            Time = DateTime.UtcNow
        };

        var inputDto = new DeviceSensorDataDto
        {
            DeviceId = deviceId.ToString(),
            Temperature = sensorReading.Temperature,
            Humidity = sensorReading.Humidity,
            AirPressure = sensorReading.AirPressure,
            AirQuality = sensorReading.AirQuality,
            Time = sensorReading.Time
        };

        var greenhouseService = _scopedServiceProvider.GetRequiredService<IGreenhouseDeviceService>();

        // Act: Add the data to DB and broadcast to WebSocket clients
        await greenhouseService.AddToDbAndBroadcast(inputDto);

        await Task.Delay(2000); // Allow time for async DB and WebSocket operations

        // Assert - WebSocket: Verify that the broadcast message contains the expected event type
        var receivedDtos = wsClient.ReceivedMessages
            .Select(str => JsonSerializer.Deserialize<BaseDto>(str))
            .Where(dto => dto != null)
            .ToList();

        Assert.That(
            receivedDtos.Any(dto => dto!.eventType == "ServerBroadcastsLiveDataToDashboard"),
            Is.True,
            "Expected WebSocket broadcast with event type 'ServerBroadcastsLiveDataToDashboard' not received."
        );

        // Assert - Database: Verify that the sensor data was saved in the database
        var dbCtx = _scopedServiceProvider.GetRequiredService<MyDbContext>();
        var matchingLogs = dbCtx.SensorHistories
            .Where(log => log.DeviceId == deviceId && log.Time == sensorReading.Time)
            .ToList();

        Assert.That(matchingLogs.Count > 0,
            $"Expected at least one SensorHistory for device ID {deviceId}, but found none.");
    }
}