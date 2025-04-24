/*using System.Text.Json;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.BroadcastModels;
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
                builder.ConfigureServices(services => { services.DefaultTestConfig(); });
            });

        _httpClient = factory.CreateClient();
        _scopedServiceProvider = factory.Services.CreateScope().ServiceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
    }

    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;


    /*[Test]
    public async Task WhenServerReceivesTimeSeriesData_ServerSavesInDbAndBroadcastsToClient()
    {
        //Arrange
        var connectionManager = _scopedServiceProvider.GetService<IConnectionManager>();
        var wsClient = _scopedServiceProvider.GetService<TestWsClient>();
        await connectionManager.AddToTopic(StringConstants.Dashboard, wsClient.WsClientId);
        var deviceId = "TestDevice" + new Random().NextDouble() * 1234;
        var testDeviceLogObject = new DeviceLogDto
        {
            DeviceId = deviceId,
            Unit = "Celcius",
            Value = 20
        };
        //Act
        await _scopedServiceProvider.GetRequiredService<IWeatherStationService>()
            .AddToDbAndBroadcast(testDeviceLogObject);
        await Task.Delay(1000);

        //Assert
        var receivedDtos = wsClient.ReceivedMessages
            .Select(str => JsonSerializer.Deserialize<BaseDto>(str));

        if (!receivedDtos.Any(baseDto => baseDto.eventType == nameof(ServerBroadcastsLiveDataToDashboard)))
            throw new Exception("Did not receive any websocket messages indicating a broadcast to the expected client");

        var dbCtx = _scopedServiceProvider.GetRequiredService<MyDbContext>();
        if (!dbCtx.Devicelogs.Any(log => log.Deviceid.Equals(deviceId)))
            throw new Exception("Expected a log form device of ID " + deviceId + " but only found: " +
                                JsonSerializer.Serialize(dbCtx.Devicelogs.ToList()));
    }#1#
}*/