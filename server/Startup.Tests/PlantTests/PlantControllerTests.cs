using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.PlantTests;

[TestFixture]
public class PlantControllerTests : WebApplicationFactory<Program>
{
    [SetUp]
    public async Task Setup()
    {
        _client = CreateClient();

        _testUser = MockObjects.GetUser();
        using var seedScope = Services.CreateScope();
        var seedDb = seedScope.ServiceProvider.GetRequiredService<MyDbContext>();
        seedDb.Users.Add(_testUser);
        await seedDb.SaveChangesAsync();
        // Login to get JWT
        var loginResp = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { _testUser.Email, Password = "pass" }
        );
        loginResp.EnsureSuccessStatusCode();
        var authDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        _jwt = authDto!.Jwt;
        _client.DefaultRequestHeaders.Add("Authorization", _jwt);
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
    }

    private HttpClient _client;
    private User _testUser = null!;
    private string _jwt = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: false); });
    }

    [Test]
    public async Task CreatePlant_PersistsAndReturnsPlant()
    {
        //arrange
        var createDto = new PlantCreateDto
        {
            PlantName = "Basil",
            PlantType = "Herb",
            PlantNotes = "Loves sunshine",
            Planted = DateTime.UtcNow.Date,
            WaterEvery = 3,
            IsDead = false
        };

        //act
        var resp = await _client.PostAsJsonAsync($"api/Plant/{PlantController.CreatePlantRoute}", createDto);

        //assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dto = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        Assert.Multiple(() =>
        {
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.PlantName, Is.EqualTo(createDto.PlantName));
            Assert.That(dto.PlantType, Is.EqualTo(createDto.PlantType));
            Assert.That(dto.PlantNotes, Is.EqualTo(createDto.PlantNotes));
            Assert.That(dto.PlantId, Is.Not.EqualTo(Guid.Empty));
        });
    }

    [Test]
    public async Task DeletePlant_ReturnsOk()
    {
        // arrange – create a plant first
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Rosemary",
                PlantType = "Herb",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });

        create.EnsureSuccessStatusCode();
        var id = (await create.Content
            .ReadFromJsonAsync<PlantResponseDto>())!.PlantId;

        var markAsDead = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);
        markAsDead.EnsureSuccessStatusCode();

        //act
        var resp = await _client.DeleteAsync($"api/Plant/{PlantController.DeletePlantRoute}?plantId={id}");

        //assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task GetAllPlants_ReturnsOnlyUsersPlants_IgnoresOtherPlants()
    {
        //arrange
        for (var i = 0; i < 2; i++)
            await _client.PostAsJsonAsync(
                $"api/Plant/{PlantController.CreatePlantRoute}",
                new PlantCreateDto
                {
                    PlantName = $"Plant {i}",
                    PlantType = "Test",
                    PlantNotes = "",
                    Planted = DateTime.UtcNow.Date
                });

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);

        await otherUserClient.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Intruder",
                PlantType = "Other",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });

        //act
        var resp = await _client.GetAsync($"api/Plant/{PlantController.GetPlantsRoute}?userId={_testUser.UserId}");

        //assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var plants = await resp.Content.ReadFromJsonAsync<List<PlantResponseDto>>();
        Assert.That(plants, Has.Count.EqualTo(2));
        Assert.That(plants!.All(p => p.PlantName.StartsWith("Plant")));
    }

    [Test]
    public async Task EditPlant_UpdateChosenFields_ShouldReturnSuccessfully()
    {
        // Arrange
        var createResp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Parsley",
                PlantType = "Herb",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });

        var created = await createResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var plantId = created!.PlantId;

        var patch = new PlantEditDto
        {
            PlantName = "Flat‑leaf Parsley",
            PlantType = "Herb",
            PlantNotes = "Move to bigger pot"
        };

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={plantId}",
            patch);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var updated = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        Assert.Multiple(() =>
        {
            Assert.That(updated!.PlantName, Is.EqualTo(patch.PlantName));
            Assert.That(updated.PlantNotes, Is.EqualTo(patch.PlantNotes));
            Assert.That(updated.PlantType, Is.EqualTo(created.PlantType));
        });
    }

    [Test]
    public async Task GetPlant_ReturnsSinglePlant()
    {
        // arrange – create a plant first
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Rosemary",
                PlantType = "Herb",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });

        var created = (await create.Content.ReadFromJsonAsync<PlantResponseDto>())!;
        var id = created.PlantId;

        // act
        var resp = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");

        // assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dto = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        Assert.Multiple(() =>
        {
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.PlantId, Is.EqualTo(id));
            Assert.That(dto.PlantName, Is.EqualTo("Rosemary"));
            Assert.That(dto.PlantType, Is.EqualTo("Herb"));
        });
    }

    [Test]
    public async Task WaterPlant_ShouldWaterSuccessfully()
    {
        // arrange
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Aloe",
                PlantType = "Succulent",
                Planted = DateTime.UtcNow.Date
            });
        var created = (await create.Content.ReadFromJsonAsync<PlantResponseDto>())!;
        var id = created.PlantId;

        // act – water that single plant
        var patch = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterPlantRoute}?plantId={id}", null);

        // assert response
        Assert.That(patch.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // fetch again to verify
        var fetch = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");
        var dto = (await fetch.Content.ReadFromJsonAsync<PlantResponseDto>())!;

        Assert.That(dto.LastWatered, Is.Not.Null.And.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task WaterAllPlants_WatersAllUsersPlants_ShouldSucceed()
    {
        // arrange – two plants for test‑user
        for (var i = 0; i < 2; ++i)
            await _client.PostAsJsonAsync(
                $"api/Plant/{PlantController.CreatePlantRoute}",
                new PlantCreateDto
                {
                    PlantName = $"Plant {i}",
                    PlantType = "Test",
                    Planted = DateTime.UtcNow.Date
                });

        // act
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterAllPlantsRoute}", null);

        // assert HTTP
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // verify DB state
        var listResp = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantsRoute}?userId={_testUser.UserId}");
        var plants = (await listResp.Content.ReadFromJsonAsync<List<PlantResponseDto>>())!;

        Assert.Multiple(() =>
        {
            Assert.That(plants, Has.Count.EqualTo(2));
            Assert.That(plants.All(p => p.LastWatered != null));
        });
    }

    [Test]
    public async Task MarkPlantAsDead_FlagsPlantAsDead_ShouldSucceedAndReturnOk()
    {
        // arrange
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Cilantro",
                PlantType = "Herb",
                Planted = DateTime.UtcNow.Date
            });
        var created = (await create.Content.ReadFromJsonAsync<PlantResponseDto>())!;
        var id = created.PlantId;

        // act – mark as dead
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);

        // assert HTTP
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // fetch again
        var fetch = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");
        var dto = (await fetch.Content.ReadFromJsonAsync<PlantResponseDto>())!;

        Assert.That(dto.IsDead, Is.True);
    }

    [Test]
    public async Task GetPlant_UnknownId_ReturnsNotFound()
    {
        var resp = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={Guid.NewGuid()}");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EditPlant_UnknownId_ReturnsNotFound()
    {
        var patch = new PlantEditDto
            { PlantName = "This is a Test", PlantType = "This is a test plant", PlantNotes = "should not matter" };

        var resp = await _client.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={Guid.NewGuid()}",
            patch);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task WaterPlant_UnknownId_ReturnsNotFound()
    {
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterPlantRoute}?plantId={Guid.NewGuid()}",
            null);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreatePlant_MissingRequiredFields_ReturnsBadRequest()
    {
        var badDto = new
        {
            // PlantName intentionally omitted
            PlantType = "Herb",
            Planted = DateTime.UtcNow.Date
        };

        var resp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}", badDto);

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAllPlants_NoJwt_ReturnsBadRequest()
    {
        using var anonClient = CreateClient();
        var resp = await anonClient.GetAsync(
            $"api/Plant/{PlantController.GetPlantsRoute}?userId={_testUser.UserId}");

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}