using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.Request;
using Core.Domain.Entities;
using FluentValidation;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.UserTests;

[TestFixture]
public class UserControllerTest
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;
    private IServiceScopeFactory _scopeFactory;
    
    private User _testUser;
    
    private Mock<IValidator<PatchUserEmailDto>> _emailValidatorMock;
    private Mock<IValidator<PatchUserPasswordDto>> _passwordValidatorMock;
    
    [OneTimeSetUp]
    public void Setup()
    {
        _emailValidatorMock = new Mock<IValidator<PatchUserEmailDto>>();
        _passwordValidatorMock = new Mock<IValidator<PatchUserPasswordDto>>();

        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                    services.DefaultTestConfig());
            });

        _client = _factory.CreateClient();
        _scopeFactory = _factory.Services.GetRequiredService<IServiceScopeFactory>();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [SetUp]
    public async Task BeforeEach()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        
        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();
        
        _testUser = MockObjects.GetUser();
        db.Users.Add(_testUser);
        await db.SaveChangesAsync();
        
        // Log in and set JWT
        var loginResp = await _client.PostAsJsonAsync(
            "/api/auth/login", 
            new { _testUser.Email, Password = "Secret25!" });
        loginResp.EnsureSuccessStatusCode();
        
        var authDto = await loginResp.Content
            .ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(authDto, Is.Not.Null);
        
        _client.DefaultRequestHeaders.Remove("Authorization");
        _client.DefaultRequestHeaders.Add(
            "Authorization", 
            authDto.Jwt);
    }

    [Test]
    public async Task GetUser_ReturnsCurrentUser()
    {
        // Act
        var resp = await _client.GetAsync($"api/User/{UserController.GetUserRoute}");
        resp.EnsureSuccessStatusCode();
        var user = await resp.Content.ReadFromJsonAsync<User>();
        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.Multiple(() =>
        {
            
            Assert.That(user.Email, Is.EqualTo(_testUser.Email));
            Assert.That(user.FirstName, Is.EqualTo(_testUser.FirstName));
            Assert.That(user.LastName, Is.EqualTo(_testUser.LastName));
        });
    }

    [Test]
    public async Task DeleteUser_ShouldReturnOk_AndMarkUserAsDeleted()
    {
        // Act
        var response = await _client.DeleteAsync($"api/User/{UserController.DeleteUserRoute}");

        // Assert status code
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        // Check updated user in DB
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
    
        var userInDb = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "Deleted@User.com");

        Assert.That(userInDb, Is.Not.Null, "Expected user to still exist but marked as deleted");

        Assert.Multiple(() =>
        {
            Assert.That(userInDb.FirstName, Is.EqualTo("Deleted"));
            Assert.That(userInDb.LastName, Is.EqualTo("User"));
            Assert.That(userInDb.Country, Is.EqualTo("-"));
            Assert.That(userInDb.Birthday, Is.EqualTo(DateTime.MinValue));
        });
    }

    [Test]
    public async Task PatchUserEmail_ShouldReturnOk()
    {
        var patchDto = new PatchUserEmailDto
        {
            OldEmail = _testUser.Email,
            NewEmail = "newemail@example.com"
        };
        
        _emailValidatorMock.Setup(v => v.ValidateAsync(patchDto, CancellationToken.None));

        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserEmailRoute}", patchDto);
        response.EnsureSuccessStatusCode();
        
        _client.DefaultRequestHeaders.Remove("Authorization");
        _testUser.Email = patchDto.NewEmail;
        
        var loginResp = await _client.PostAsJsonAsync("/api/auth/login", new { _testUser.Email, Password = "Secret25!" });
        loginResp.EnsureSuccessStatusCode();
        var authDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(authDto, Is.Not.Null);
        _client.DefaultRequestHeaders.Add("Authorization", authDto.Jwt);
        
        
        var getResp = await _client.GetAsync($"api/User/{UserController.GetUserRoute}");
        getResp.EnsureSuccessStatusCode();
        
        var user = await getResp.Content.ReadFromJsonAsync<User>();
        Assert.That(user, Is.Not.Null);
        Assert.That(user.Email, Is.EqualTo(patchDto.NewEmail));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnOk()
    {
        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "Secret25!",
            NewPassword = "newPass123!"
        };

        _passwordValidatorMock.Setup(v => v.ValidateAsync(patchDto, CancellationToken.None));

        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserPasswordRoute}", patchDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "wrongPass!25",
            NewPassword = "newPass123!"
        };

        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserPasswordRoute}", patchDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task DeleteUser_ShouldReturnBadRequest_WhenNoJwtProvided()
    {
        var unAuthorizedClient = _factory.CreateClient(); // No JWT
        var response = await unAuthorizedClient.DeleteAsync($"api/User/{UserController.DeleteUserRoute}");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PatchUserEmail_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        // Delete the user manually to simulate "not found"
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = db.Users.First();
        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var patchDto = new PatchUserEmailDto
        {
            OldEmail = "AAAAAAAAAAAAAAAAA@JEGEKSISTERERIKKE.DK",
            NewEmail = "new@example.com"
        };

        // Act
        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserEmailRoute}", patchDto);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchUserEmail_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange invalid DTO
        var dto = new PatchUserEmailDto { OldEmail = _testUser.Email, NewEmail = "" };
        _emailValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()));

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/User/{UserController.PatchUserEmailRoute}", dto);
        var body = await resp.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Contain("The NewEmail field is required"));
            Assert.That(body, Does.Contain("The NewEmail field is not a valid e-mail address"));
        });
    }
    
    [Test]
    public async Task PatchUserPassword_ShouldReturnBadRequest_WhenValidationFails()
    {
        // Arrange invalid DTO
        var dto = new PatchUserPasswordDto { OldPassword = "Secret25!", NewPassword = "" };
        _passwordValidatorMock
            .Setup(v => v.ValidateAsync(dto, It.IsAny<CancellationToken>()));

        // Act
        var resp = await _client.PatchAsJsonAsync(
            $"api/User/{UserController.PatchUserPasswordRoute}", dto);
        var body = await resp.Content.ReadAsStringAsync();

        Assert.Multiple(() =>
        {
            // Assert
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(body, Does.Contain("The NewPassword field is required"));
        });
    }
    
    [Test]
    public async Task PatchUserEmail_ShouldReturnBadRequest_WhenEmailIsInvalid()
    {
        var patchDto = new PatchUserEmailDto { NewEmail = "" }; // an empty email should trigger ArgumentException
        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserEmailRoute}", patchDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        using var scope = _scopeFactory.CreateScope();

        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var user = db.Users.First();

        db.Users.Remove(user);

        await db.SaveChangesAsync();

        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "Secret25!",
            NewPassword = "newPass123!"
        };

        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserPasswordRoute}", patchDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }

    [Test]
    public async Task PatchUserPassword_ShouldReturnBadRequest_WhenNewPasswordIsInvalid()
    {
        var patchDto = new PatchUserPasswordDto
        {
            OldPassword = "pass",
            NewPassword = "" // invalid password
        };

        var response = await _client.PatchAsJsonAsync($"api/User/{UserController.PatchUserPasswordRoute}", patchDto);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task DeleteUser_ShouldReturnNotFound_WhenUserDoesNotExist()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = db.Users.First();
        db.Users.Remove(user);
        await db.SaveChangesAsync();

        var response = await _client.DeleteAsync($"api/User/{UserController.DeleteUserRoute}");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}