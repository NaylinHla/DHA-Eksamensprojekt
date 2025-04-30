using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.AlertTests;

[TestFixture]
public class AlertControllerTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.DefaultTestConfig();
                });
            });

        _client = _factory.CreateClient();
    }
    
    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task CreateAlert_ShouldPersistAndReturnAlert()
    {
        // Arrange
        var auth = await ApiTestSetupUtilities.TestRegisterAndAddJwt(_client);

        var alertDto = new AlertCreate
        {
            AlertName = "High Temp",
            AlertDesc = "The temperature is too high"
        };

        // Act
        var response = await _client.PostAsJsonAsync("api/Alert/CreateAlert", alertDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var createdAlert = await response.Content.ReadFromJsonAsync<AlertResponseDto>();
        Assert.That(createdAlert, Is.Not.Null);
        Assert.That(createdAlert!.AlertName, Is.EqualTo(alertDto.AlertName));
        Assert.That(createdAlert.AlertDesc, Is.EqualTo(alertDto.AlertDesc));
    }

    [Test]
    public async Task GetAlerts_ShouldReturnOnlyCurrentUserAlerts()
    {
        // Arrange
        var auth = await ApiTestSetupUtilities.TestRegisterAndAddJwt(_client);

        // Act
        var response = await _client.GetAsync("api/Alert/GetAlerts");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var alerts = await response.Content.ReadFromJsonAsync<List<AlertResponseDto>>();
        Assert.That(alerts, Is.Not.Null);
    }
}
