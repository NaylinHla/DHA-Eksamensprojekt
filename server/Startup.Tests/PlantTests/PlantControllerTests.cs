using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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
        // Arrange
        var createDto = new PlantCreateDto
        {
            PlantName = "Basil",
            PlantType = "Herb",
            PlantNotes = "Loves sunshine",
            Planted = DateTime.UtcNow.Date,
            WaterEvery = 3,
            IsDead = false
        };

        // Act
        var resp = await _client.PostAsJsonAsync($"api/Plant/{PlantController.CreatePlantRoute}", createDto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var dto = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        Assert.Multiple(() =>
        {
            Assert.That(dto, Is.Not.Null);
            Assert.That(dto!.PlantName, Is.EqualTo(createDto.PlantName));
            Assert.That(dto.PlantType, Is.EqualTo(createDto.PlantType));
            Assert.That(dto.PlantNotes, Is.EqualTo(createDto.PlantNotes));
            Assert.That(dto.PlantId, Is.Not.EqualTo(Guid.Empty));
            Assert.That(dto.IsDead, Is.EqualTo(createDto.IsDead));
            Assert.That(dto.WaterEvery, Is.EqualTo(createDto.WaterEvery));
            Assert.That(dto.Planted, Is.EqualTo(createDto.Planted));
        });
        var checkDb = await _client.GetAsync($"api/Plant/{PlantController.GetPlantRoute}?plantId={dto.PlantId}");
        Assert.That(checkDb.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task DeletePlant_ReturnsOk()
    {
        // Arrange
        var createA = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Rosemary",
                PlantType = "Herb",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });

        createA.EnsureSuccessStatusCode();
        var plantA = (await createA.Content.ReadFromJsonAsync<PlantResponseDto>())!.PlantId;

        var markAsDead = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={plantA}", null);
        markAsDead.EnsureSuccessStatusCode();
        
        var createB = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Mint",
                PlantType = "Herb",
                PlantNotes = "",
                Planted = DateTime.UtcNow.Date
            });
        createB.EnsureSuccessStatusCode();
        var plantB = (await createB.Content.ReadFromJsonAsync<PlantResponseDto>())!.PlantId;

        // Act
        var deleteResp = await _client.DeleteAsync($"api/Plant/{PlantController.DeletePlantRoute}?plantId={plantA}");
        deleteResp.EnsureSuccessStatusCode();
        
        // Assert
        var getA = await _client.GetAsync($"api/Plant/{PlantController.GetPlantRoute}?plantId={plantA}");
        var getB = await _client.GetAsync($"api/Plant/{PlantController.GetPlantRoute}?plantId={plantB}");
        
        Assert.Multiple(() =>
        {
            Assert.That(deleteResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getA.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(getB.StatusCode, Is.EqualTo(HttpStatusCode.OK));;
        });

        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        await Assert.MultipleAsync(async() =>
        {
            Assert.That(await db.UserPlants.AnyAsync(up => up.PlantId == plantA), Is.False);
            Assert.That(await db.UserPlants.AnyAsync(up => up.PlantId == plantB), Is.True);
            Assert.That(await db.Plants.AnyAsync(p => p.PlantId == plantA), Is.False);
        });
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
                WaterEvery = 3,
                IsDead = false,
                Planted = DateTime.UtcNow.Date
            });

        var created = await createResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var plantId = created!.PlantId;

        var patch = new PlantEditDto
        {
            PlantName = "Flat‑leaf Parsley",
            PlantType = "Fungus",
            PlantNotes = "Move to bigger pot",
            WaterEvery = 5,
            LastWatered = DateTime.UtcNow.Date
        };

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={plantId}",
            patch);

        var getResp = await _client.GetAsync($"api/Plant/{PlantController.GetPlantRoute}?plantId={plantId}");
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        });
        
        var updated = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var refetchedPlantFromDb = await getResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        Assert.Multiple(() =>
        {
            Assert.That(updated!.PlantName, Is.EqualTo(patch.PlantName));
            Assert.That(updated.PlantNotes, Is.EqualTo(patch.PlantNotes));
            Assert.That(updated.PlantType, Is.EqualTo(patch.PlantType));
            Assert.That(updated.WaterEvery, Is.EqualTo(patch.WaterEvery));
            Assert.That(updated.LastWatered, Is.EqualTo(patch.LastWatered));
        });
        Assert.Multiple(() =>
        {
            Assert.That(refetchedPlantFromDb!.PlantName, Is.EqualTo(patch.PlantName));
            Assert.That(refetchedPlantFromDb.PlantNotes, Is.EqualTo(patch.PlantNotes));
            Assert.That(refetchedPlantFromDb.PlantType, Is.EqualTo(patch.PlantType));
            Assert.That(refetchedPlantFromDb.WaterEvery, Is.EqualTo(patch.WaterEvery));
            Assert.That(refetchedPlantFromDb.LastWatered, Is.EqualTo(patch.LastWatered));
        });
    }

    [Test]
    public async Task EditPlant_NullValueInFields_ShouldReturnSuccessfully()
    {
        // Arrange
        var createResp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName = "Parsley",
                PlantType = "Herb",
                PlantNotes = "",
                WaterEvery = 3,
                IsDead = false,
                Planted = DateTime.UtcNow.Date
            });

        var created = await createResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var plantId = created!.PlantId;

        var patch = new PlantEditDto
        {
            PlantName = null,
            PlantType = null,
            PlantNotes = null,
            WaterEvery = null,
        };

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={plantId}",
            patch);

        // Assert

        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var updated = await resp.Content.ReadFromJsonAsync<PlantResponseDto>();
        
        Assert.That(updated, Is.Not.EqualTo(null));
       
        Assert.Multiple(() =>
        {
            Assert.That(updated.PlantName, Is.Not.EqualTo(patch.PlantName));
            Assert.That(updated.PlantNotes, Is.Not.EqualTo(patch.PlantNotes));
            Assert.That(updated.PlantType, Is.Not.EqualTo(patch.PlantType));
            Assert.That(updated.WaterEvery, Is.Not.EqualTo(patch.WaterEvery));
            Assert.That(updated.LastWatered, Is.EqualTo(created.LastWatered));
            
        });
    }
    
    [Test]
    public async Task GetPlant_ReturnsSinglePlant()
    {
        // Arrange
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

        // Act
        var resp = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");

        // Assert
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
        // Arrange
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

        // Act
        var patch = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterPlantRoute}?plantId={id}", null);

        // Assert
        Assert.That(patch.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var fetch = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");
        var dto = (await fetch.Content.ReadFromJsonAsync<PlantResponseDto>())!;

        Assert.That(dto.LastWatered, Is.Not.Null.And.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public async Task WaterAllPlants_WatersAllUsersPlants_ShouldSucceed()
    {
        // Arrange
        for (var i = 0; i < 2; ++i)
            await _client.PostAsJsonAsync(
                $"api/Plant/{PlantController.CreatePlantRoute}",
                new PlantCreateDto
                {
                    PlantName = $"Plant {i}",
                    PlantType = "Test",
                    Planted = DateTime.UtcNow.Date
                });

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        
        for (var i = 0; i < 2; ++i)
            await otherUserClient.PostAsJsonAsync(
                $"api/Plant/{PlantController.CreatePlantRoute}",
                new PlantCreateDto
                {
                    PlantName = $"Plant {i}",
                    PlantType = "Test",
                    Planted = DateTime.UtcNow.Date
                });
        
        // Act
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterAllPlantsRoute}?userId={_testUser.UserId}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

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
        // Arrange
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

        // Act
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var fetch = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={id}");
        var dto = (await fetch.Content.ReadFromJsonAsync<PlantResponseDto>())!;

        Assert.That(dto.IsDead, Is.True);
    }

    [Test]
    public async Task DeletePlant_DifferentPlantThatDoesNotExist_ShouldFailAndReturnNotFound()
    {
        // Arrange
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName   = "Rosemary",
                PlantType   = "Herb",
                PlantNotes  = "",
                Planted     = DateTime.UtcNow.Date
            });

        create.EnsureSuccessStatusCode();
        var id = (await create.Content
            .ReadFromJsonAsync<PlantResponseDto>())!.PlantId;
        
        var markAsDead = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);
        markAsDead.EnsureSuccessStatusCode();

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
                
        var fakePlantId = Guid.NewGuid();
        
        // Act
        var resp = await otherUserClient.DeleteAsync($"api/Plant/{PlantController.DeletePlantRoute}?plantId={fakePlantId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task DeletePlant_DifferentUserAttemptsToDeletePlant_ShouldFailAndReturnUnauthorized()
    {
        // Arrange
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName   = "Rosemary",
                PlantType   = "Herb",
                PlantNotes  = "",
                Planted     = DateTime.UtcNow.Date
            });

        create.EnsureSuccessStatusCode();
        var id = (await create.Content
            .ReadFromJsonAsync<PlantResponseDto>())!.PlantId;
        
        var markAsDead = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);
        markAsDead.EnsureSuccessStatusCode();

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
                
        // Act
        var resp = await otherUserClient.DeleteAsync($"api/Plant/{PlantController.DeletePlantRoute}?plantId={id}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task DeletePlant_TryAndDeleteAlivePlant_ShouldReturnBadRequest()
    {
        // Arrange
        var create = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName   = "Rosemary",
                PlantType   = "Herb",
                PlantNotes  = "",
                Planted     = DateTime.UtcNow.Date
            });

        create.EnsureSuccessStatusCode();
        var id = (await create.Content
            .ReadFromJsonAsync<PlantResponseDto>())!.PlantId;

        // Act
        var resp = await _client.DeleteAsync($"api/Plant/{PlantController.DeletePlantRoute}?plantId={id}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
    
    
    [Test]
    public async Task GetPlant_UnknownId_ReturnsNotFound()
    {
        // Act
        var resp = await _client.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={Guid.NewGuid()}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EditPlant_UnknownId_ReturnsNotFound()
    {
        // Arrange
        var patch = new PlantEditDto
            { PlantName = "This is a Test", PlantType = "This is a test plant", PlantNotes = "should not matter" };

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={Guid.NewGuid()}",
            patch);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task EditPlant_UpdateChosenFields_ShouldReturnUnauthorized()
    {
        // Arrange
        var createResp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName   = "Parsley",
                PlantType   = "Herb",
                PlantNotes  = "",
                Planted     = DateTime.UtcNow.Date
            });

        var created = await createResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var plantId = created!.PlantId;

        var patch = new PlantEditDto
        {
            PlantName  = "Flat‑leaf Parsley",
            PlantType  = "Herb",
            PlantNotes = "Move to bigger pot"
        };
        
        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        
        // Act
        var resp = await otherUserClient.PatchAsJsonAsync(
            $"api/Plant/{PlantController.EditPlantRoute}?plantId={plantId}",
            patch);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task WaterPlant_UnknownId_ReturnsNotFound()
    {
        // Act
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.WaterPlantRoute}?plantId={Guid.NewGuid()}",
            null);
        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task CreatePlant_MissingRequiredFields_ReturnsBadRequest()
    {
        // Arrange
        var badDto = new
        {
            // PlantName intentionally omitted
            PlantType = "Herb",
            Planted = DateTime.UtcNow.Date
        };

        // Act
        var resp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}", badDto);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task GetAllPlants_NoJwt_ReturnsUnauthorized()
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
        // Act
        using var anonClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(anonClient);
        var resp = await anonClient.GetAsync($"api/Plant/{PlantController.GetPlantsRoute}?userId={_testUser.UserId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task GetPlant_WrongUserId_ShouldReturnUnauthorized()
    {
        // Arrange
        var createResp = await _client.PostAsJsonAsync(
            $"api/Plant/{PlantController.CreatePlantRoute}",
            new PlantCreateDto
            {
                PlantName   = "Parsley",
                PlantType   = "Herb",
                PlantNotes  = "",
                Planted     = DateTime.UtcNow.Date
            });

        var created = await createResp.Content.ReadFromJsonAsync<PlantResponseDto>();
        var plantId = created!.PlantId;
        
        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        
        // Act
        var resp = await otherUserClient.GetAsync(
            $"api/Plant/{PlantController.GetPlantRoute}?plantId={plantId}");

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task MarkPlantAsDead_FlagNullPlant_ShouldReturnKeyNotFound()
    { 
        // Act
        var resp = await _client.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={Guid.NewGuid()}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
    
    [Test]
    public async Task MarkPlantAsDead_FlagAnotherPersonsPlant_ShouldReturnUnauthorized()
    { 
        // Arrange
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

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        
        // Act
        var resp = await otherUserClient.PatchAsync(
            $"api/Plant/{PlantController.PlantIsDeadRoute}?plantId={id}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task WaterPlant_WrongUserTriesToWater_ShouldReturnUnauthorized()
    { 
        // Arrange
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
        
        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        
        // Act
        var resp = await otherUserClient.PatchAsync(
            $"api/Plant/{PlantController.WaterPlantRoute}?plantId={id}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task WaterAllPlants_WatersAllOtherUsersPlants_ShouldReturnUnauthorized()
    {
        // Arrange
        for (var i = 0; i < 2; ++i)
            await _client.PostAsJsonAsync(
                $"api/Plant/{PlantController.CreatePlantRoute}",
                new PlantCreateDto
                {
                    PlantName = $"Plant {i}",
                    PlantType = "Test",
                    Planted = DateTime.UtcNow.Date
                });

        var otherUserClient = CreateClient();
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(otherUserClient);
        // Act
        var resp = await otherUserClient.PatchAsync(
            $"api/Plant/{PlantController.WaterAllPlantsRoute}?userId={_testUser.UserId}", null);

        // Assert
        Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
}