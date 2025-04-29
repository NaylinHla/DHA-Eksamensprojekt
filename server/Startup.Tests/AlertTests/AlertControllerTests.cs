using System.Net;
using System.Net.Http.Json;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.AlertTests;

public class AlertControllerTests : WebAppTestBase
{
    [Test]
    public async Task CreateAlert_ShouldPersistAndReturnAlert()
    {
        // Arrange
        var auth = await ApiTestSetupUtilities.TestRegisterAndAddJwt(Client);

        var alertDto = new AlertCreate
        {
            AlertName = "High Temp",
            AlertDesc = "The temperature is too high"
        };

        // Act
        var response = await Client.PostAsJsonAsync("CreateAlert", alertDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var createdAlert = await response.Content.ReadFromJsonAsync<AlertResponseDto>();
        createdAlert.Should().NotBeNull();
        createdAlert!.AlertName.Should().Be(alertDto.AlertName);
        createdAlert.AlertDesc.Should().Be(alertDto.AlertDesc);
    }

    [Test]
    public async Task GetAlerts_ShouldReturnOnlyCurrentUserAlerts()
    {
        // Arrange
        var auth = await ApiTestSetupUtilities.TestRegisterAndAddJwt(Client);

        // Act
        var response = await Client.GetAsync("GetAlerts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var alerts = await response.Content.ReadFromJsonAsync<List<AlertResponseDto>>();
        alerts.Should().NotBeNull();
    }
}
