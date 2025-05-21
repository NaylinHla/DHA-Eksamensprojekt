using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Interfaces.Infrastructure.Websocket;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using HiveMQtt.MQTT5.Types;
using Infrastructure.Postgres.Scaffolding;
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
                builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: true); });
            });

        _httpClient = factory.CreateClient();
        _scopedServiceProvider = factory.Services.CreateScope().ServiceProvider;
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient.Dispose();
    }

    private HttpClient _httpClient;
    private IServiceProvider _scopedServiceProvider;

    [Test]
    public async Task WhenSubscribingToTopicUsingRestRequest_ResponseIsOkAndConnectionManagerHasAddedToTopic()
    {
        //Arrange
        var connectionManager = _scopedServiceProvider.GetService<IConnectionManager>();
        Assert.That(connectionManager, Is.Not.Null);
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(await connectionManager.GetMembersFromTopicId(StringConstants.Dashboard), Is.Empty);
        });

        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        
        //Act
        var subscribeToTopicRequest = await _httpClient.PostAsJsonAsync(
            SubscriptionController.SubscriptionRoute, new ChangeSubscriptionDto
            {
                ClientId = _scopedServiceProvider.GetRequiredService<TestWsClient>().WsClientId,
                TopicIds = [StringConstants.Dashboard]
            });

        Assert.Multiple(() =>
        {
            //Assert
            Assert.That(subscribeToTopicRequest.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(connectionManager.GetMembersFromTopicId(StringConstants.Dashboard).Result, Has.Count.EqualTo(1));
        });
    }

    [Test]
    public async Task WhenUnsubscribingFromTopicUsingRestRequest_ResponseIsOkAndConnectionManagerRemovesFromTopic()
    {
        // Arrange
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);

        // First subscribe
        await _httpClient.PostAsJsonAsync(
            SubscriptionController.SubscriptionRoute,
            new ChangeSubscriptionDto
            {
                ClientId = testWsClient.WsClientId,
                TopicIds = [StringConstants.Dashboard]
            });

        // Act
        var unsubscribeResp = await _httpClient.PostAsJsonAsync(
            SubscriptionController.UnsubscribeRoute,
            new ChangeSubscriptionDto
            {
                ClientId = testWsClient.WsClientId,
                TopicIds = [StringConstants.Dashboard]
            });

        // Assert
        Assert.That(unsubscribeResp.IsSuccessStatusCode, Is.True);
        var membersAfter = await connectionManager.GetMembersFromTopicId(StringConstants.Dashboard);
        Assert.That(membersAfter, Does.Not.Contain(testWsClient.WsClientId));
    }

    [Test]
    public async Task WhenBroadcastingExampleEventToTopic_AllSubscribersReceiveIt()
    {
        // Arrange
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await connectionManager.AddToTopic("ExampleTopic", testWsClient.WsClientId);

        var dto = new ExampleBroadcastDto
        {
            eventType = "TestBroadcast",
            Message = "Hello Subscribers!"
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync(
            SubscriptionController.ExampleBroadcastRoute, dto);

        // Assert
        Assert.That(response.IsSuccessStatusCode, Is.True);
    }


    [Test]
    public async Task WhenAdminChangesDevicePreferencesFromWebDashboard_MqttClientPublishesToEdgeDevice()
    {
        // Seed test user and device
        var testUser = MockObjects.GetUser();
        using (var scope = _scopedServiceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
            db.Users.Add(testUser);
            await db.SaveChangesAsync();
        }

        // Log in and set JWT
        var loginResp = await _httpClient.PostAsJsonAsync("/api/auth/login", new { testUser.Email, Password = "pass" });
        loginResp.EnsureSuccessStatusCode();
        var authDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(authDto, Is.Not.Null);
        _httpClient.DefaultRequestHeaders.Add("Authorization", authDto.Jwt);

        // Use the seeded device ID
        var seededDeviceId = testUser.UserDevices.First().DeviceId;

        //Arrange a MQTT client to perform publishing on the REST trigger
        var testMqttClient = _scopedServiceProvider.GetService<TestMqttClient>();
        Assert.That(testMqttClient, Is.Not.Null);
        var topic = StringConstants.Device + "/" + seededDeviceId + "/" + StringConstants.ChangePreferences;
        await testMqttClient.MqttClient.SubscribeAsync(topic, QualityOfService.ExactlyOnceDelivery);

        //Arrange WS client
        var testWsClient = _scopedServiceProvider.GetRequiredService<TestWsClient>();
        var connectionManager = _scopedServiceProvider.GetRequiredService<IConnectionManager>();
        await connectionManager.AddToTopic(StringConstants.Dashboard, testWsClient.WsClientId);

        //Rest DTO
        var changePreferencesDto = new AdminChangesPreferencesDto
        {
            DeviceId = seededDeviceId.ToString(),
            Interval = "60"
        };

        //Act
        await _httpClient.PostAsJsonAsync(
            $"api/UserDevice/{UserDeviceController.AdminChangesPreferencesRoute}",
            changePreferencesDto);
        await Task.Delay(3000); // Hardcoded delay to account for network overhead to the edge device

        var actualObjectReceivedByMqttDevice =
            JsonSerializer.Deserialize<AdminChangesPreferencesDto>(testMqttClient.ReceivedMessages.First(),
                JsonSerializerOptions.Web);
        var comparison = new CompareLogic().Compare(actualObjectReceivedByMqttDevice, changePreferencesDto);
        Assert.That(comparison.AreEqual, Is.True);

    }
}