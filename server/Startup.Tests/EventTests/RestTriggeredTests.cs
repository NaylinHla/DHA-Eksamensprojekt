/*using System.Net.Http.Json;
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using HiveMQtt.MQTT5.Types;
using KellermanSoftware.CompareNetObjects;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EventTests;

[TestFixture]
public class RestTriggeredTests
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
    public async Task WhenSubscribingToTopicUsingRestRequest_ResponseIsOkAndConnectionManagerHasAddedToTopic()
    {
        //Arrange
        var connectionManager = _scopedServiceProvider.GetService<IConnectionManager>();
        var initialMembers = await connectionManager.GetMembersFromTopicId(StringConstants.Dashboard);
        if (initialMembers.Count != 0)
            throw new Exception("Initial members in topic should be 0, but it was: " +
                                JsonSerializer.Serialize(initialMembers));
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);


        //Act
        var subscribeToTopicRequest = await _httpClient.PostAsJsonAsync(
            SubscriptionController.SubscriptionRoute, new ChangeSubscriptionDto
            {
                ClientId = _scopedServiceProvider.GetRequiredService<TestWsClient>().WsClientId,
                TopicIds = new List<string> { StringConstants.Dashboard }
            });

        //Assert
        if (!subscribeToTopicRequest.IsSuccessStatusCode)
            throw new Exception("Http response from subscription request indicates a failure to subscribe: " +
                                await subscribeToTopicRequest.Content.ReadAsStringAsync());
        var members = await connectionManager.GetMembersFromTopicId(StringConstants.Dashboard);
        if (members.Count != 1)
            throw new Exception("Expected exactly one subscriber to topic " + StringConstants.Dashboard +
                                ", but this is the topic members: " + JsonSerializer.Serialize(members));
    }#1#


    /*[Test]
    public async Task WhenAdminChangesDevicePreferencesFromWebDashboard_MqttClientPublishesToEdgeDevice()
    {
        //Arrange MQTT client to perform publish on REST trigger
        var testMqttClient = _scopedServiceProvider.GetService<TestMqttClient>();
        var topic = StringConstants.Device + "/" + testMqttClient.DeviceId + "/" + StringConstants.ChangePreferences;
        await testMqttClient.MqttClient.SubscribeAsync(topic, QualityOfService.ExactlyOnceDelivery);

        //Arrange WS client
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await connectionManager.AddToTopic(StringConstants.Dashboard, testWsClient.WsClientId);

        //Rest DTO
        var changePrefernecesDto = new AdminChangesPreferencesDto
        {
            DeviceId = testMqttClient.DeviceId,
            Interval = "Minute",
            Unit = "Celcius"
        };

        //Act
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        _ = await _httpClient.PostAsJsonAsync(WeatherStationController.AdminChangesPreferencesRoute,
            changePrefernecesDto);
        await Task.Delay(1000); // Hardcoded delay to account for network overhead to the edge device

        var actualObjectReceivedByMqttDevice =
            JsonSerializer.Deserialize<AdminChangesPreferencesDto>(testMqttClient.ReceivedMessages.First(),
                JsonSerializerOptions.Web);
        var comparison = new CompareLogic().Compare(actualObjectReceivedByMqttDevice, changePrefernecesDto);
        if (!comparison.AreEqual)
            throw new Exception("Comparison failed: " + comparison.DifferencesString);
    }#1#
}*/