using System.Net;
using System.Net.Http.Json;
using Application.Interfaces.Infrastructure.Websocket;
using Api.Rest.Controllers;
using Application.Interfaces;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.EventTests
{
    [Parallelizable(ParallelScope.None)]
    [TestFixture]
    public class SubscriptionControllerTests
    {
        private WebApplicationFactory<Program> _factory;
        private HttpClient _httpClient;
        private static Mock<IWebsocketSubscriptionService> CurrentSubscriptionServiceMock;
        private static Mock<IConnectionManager> CurrentConnectionManagerMock;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureTestServices(services =>
                    {
                        services.DefaultTestConfig();
                        
                        services.RemoveAll<IWebsocketSubscriptionService>();
                        services.AddTransient(_ => CurrentSubscriptionServiceMock.Object);
                        
                        services.RemoveAll<IConnectionManager>();
                        services.AddTransient(_ => CurrentConnectionManagerMock.Object);
                    });
                });

        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _factory.Dispose();
        }

        [TearDown]
        public void TearDown()
        {
            _httpClient.Dispose();
        }
        
        [SetUp]
        public async Task Setup()
        {
            CurrentSubscriptionServiceMock = new Mock<IWebsocketSubscriptionService>();
            CurrentConnectionManagerMock = new Mock<IConnectionManager>();
            
            _httpClient = _factory.CreateClient();
            
            await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        }

        [Test]
        public async Task Subscribe_ShouldCall_WebsocketSubscriptionService()
        {
            // Arrange
            var dto = new ChangeSubscriptionDto
            {
                ClientId = "client-123",
                TopicIds = ["TopicA", "TopicB"]
            };

            // Act
            var resp = await _httpClient.PostAsJsonAsync(
                SubscriptionController.SubscriptionRoute, dto);

            // Assert
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            CurrentSubscriptionServiceMock.Verify(s =>
                s.SubscribeToTopic(
                    "client-123",
                    It.Is<List<string>>(l => l.SequenceEqual(dto.TopicIds))
                ),
                Times.Once
            );
        }
        
        [Test]
        public async Task Unsubscribe_ShouldCall_WebsocketSubscriptionService()
        {
            var dto = new ChangeSubscriptionDto
            {
                ClientId = "client-123",
                TopicIds = ["TopicA", "TopicB"]
            };

            var resp = await _httpClient.PostAsJsonAsync(
                SubscriptionController.UnsubscribeRoute,
                dto);

            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            CurrentSubscriptionServiceMock.Verify(s =>
                    s.UnsubscribeFromTopic(
                        "client-123",
                        It.Is<List<string>>(l => l.SequenceEqual(dto.TopicIds))
                    ),
                Times.Once);
        }
        

        [Test]
        public async Task ExampleBroadcast_ShouldCall_ConnectionManagerBroadcast()
        {
            // Arrange
            var dto = new ExampleBroadcastDto
            {
                eventType = "MyEvent",
                Message   = "Hello!"
            };

            // Act
            var resp = await _httpClient.PostAsJsonAsync(
                SubscriptionController.ExampleBroadcastRoute, dto);

            // Assert
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            CurrentConnectionManagerMock.Verify(c =>
                    c.BroadcastToTopic(
                        "ExampleTopic",
                        It.Is<ExampleBroadcastDto>(m =>
                            m.eventType == dto.eventType &&
                            m.Message   == dto.Message
                        )
                    ),
                Times.Once,
                "Expected BroadcastToTopic to be called once with the same payload values."
            );
        }
    }
}
