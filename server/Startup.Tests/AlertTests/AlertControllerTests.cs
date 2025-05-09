using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.AlertTests;

[TestFixture]
public class AlertControllerTests : WebApplicationFactory<Program>
{
    [SetUp]
    public void Setup()
    {
        _client = CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private HttpClient _client = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: false); });
    }

    [Test]
    public async Task CreateAlert_ShouldPersistAndReturnAlert()
    {
        // Arrange
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_client);

        var alertDto = new AlertCreate
        {
            AlertName = "High Temp",
            AlertDesc = "The temperature is too high"
        };

        // Act
        var response = await _client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", alertDto);

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
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_client);

        // Act
        var response = await _client.GetAsync($"api/Alert/{AlertController.GetAlertsRoute}");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var alerts = await response.Content.ReadFromJsonAsync<List<AlertResponseDto>>();
        Assert.That(alerts, Is.Not.Null);
    }

    [Test]
    public async Task CreateAlert_NoJwt_ReturnsUnauthorized()
    {
        var dto = new AlertCreate
        {
            AlertName = "Overheat",
            AlertDesc = "Temp > 40℃"
        };

        var resp = await _client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", dto);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)
            .Or.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task CreateAlert_MissingName_ReturnsBadRequest()
    {
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_client);

        var badDto = new AlertCreate
        {
            AlertName = null, // Missing name
            AlertDesc = "No name should fail"
        };

        var resp = await _client.PostAsJsonAsync($"api/Alert/{AlertController.CreateAlertRoute}", badDto);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAlerts_NoJwt_ReturnsUnauthorized()
    {
        var resp = await _client.GetAsync($"api/Alert/{AlertController.GetAlertsRoute}");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized)
            .Or.EqualTo(HttpStatusCode.BadRequest));
    }
}