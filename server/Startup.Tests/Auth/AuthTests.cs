using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Application.Models.Dtos.RestDtos.UserSettings.Response;
using FluentValidation;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.Auth;

public class AuthTests : WebApplicationFactory<Program>
{
    private HttpClient _httpClient;
    private Mock<IValidator<AuthRegisterDto>> _mockRegisterValidator;
    private Mock<IValidator<AuthLoginDto>> _mockLoginValidator;
    
    
    [SetUp]
    public void Setup()
    {
        _mockRegisterValidator = new Mock<IValidator<AuthRegisterDto>>();
        _mockLoginValidator = new Mock<IValidator<AuthLoginDto>>();
        _httpClient = CreateClient();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(); });
    }


    [Test]
    public async Task RouteWithNoAuth_Can_Be_Accessed()
    {
        var response = await _httpClient.GetAsync("/acceptance");
        await Assert.MultipleAsync(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Accepted"));
        });
    }

    [Test]
    public async Task SecuredRouteIsBlockedWithoutJwt()
    {
        var response = await _httpClient.GetAsync(AuthController.SecuredRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }
    
    [Test]
    public async Task SecuredRouteWorksWithJwt()
    {
        await ApiTestSetupUtilities.TestRegisterAndAddJwt(_httpClient);
        var response = await _httpClient.GetAsync(AuthController.SecuredRoute);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Is.EqualTo("You are authorized to see this message"));
    }

    [Test]
    public async Task Register_Can_Register_And_Return_Jwt()
    {
        var user = MockObjects.GetUser();

        var registerDto = new AuthRegisterDto
        {
            Email = user.Email,
            Password = "Secret25!",
            FirstName = user.FirstName,
            LastName = user.LastName,
            Country = user.Country,
            Birthday = user.Birthday ?? DateTime.UtcNow.AddYears(-30)
        };

        _mockRegisterValidator.Setup(x => x.ValidateAsync(registerDto, CancellationToken.None));
        
        var response = await _httpClient.PostAsJsonAsync(AuthController.RegisterRoute, registerDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var dto = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.That(dto, Is.Not.Null);
        Assert.That(dto.Jwt, Has.Length.GreaterThan(10));
        
        _httpClient.DefaultRequestHeaders.Remove("Authorization");
        _httpClient.DefaultRequestHeaders.Add("Authorization", dto.Jwt);
        
        var getUserSettingsResp = await _httpClient.GetAsync("/api/usersettings");
        Assert.That(getUserSettingsResp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        
        var userSettings = await getUserSettingsResp.Content.ReadFromJsonAsync<UserSettingsResponseDto>();
        Assert.That(userSettings, Is.Not.Null);
        
        Assert.Multiple(() =>
        {
            Assert.That(userSettings.Celsius, Is.True);
            Assert.That(userSettings.DarkTheme, Is.False);
            Assert.That(userSettings.ConfirmDialog, Is.False);
            Assert.That(userSettings.SecretMode, Is.False);
        });
        
    }

    [Test]
    public async Task Register_With_Short_Pass_Returns_Bad_Request()
    {
        var response = await _httpClient.PostAsJsonAsync(
            AuthController.RegisterRoute, new AuthLoginDto
            {
                Email = "bob@bob.dk",
                Password = "a"
            });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }

    [Test]
    public async Task Login_Can_Login_And_Return_Jwt()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var loginDto = new AuthLoginDto
        {
            Email = user.Email,
            Password = "Secret25!"
        };
        
        _mockLoginValidator.Setup(x => x.ValidateAsync(loginDto, CancellationToken.None));

        var response = await _httpClient.PostAsJsonAsync(
            AuthController.LoginRoute, loginDto);
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task Invalid_Login_Gives_Unauthorized()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await _httpClient.PostAsJsonAsync(AuthController.LoginRoute,
            new AuthLoginDto
            {
                Email = user.Email,
                Password = "Invalidpassword1!"
            });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("Invalid login"));
    }

    [Test]
    public async Task Login_For_Non_Existing_User_Is_Unauthorized()
    {
        var response = await _httpClient.PostAsJsonAsync(AuthController.LoginRoute,
            new AuthLoginDto { Email = "bob@bob.dk", Password = "Secret25!" });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        var body = await response.Content.ReadAsStringAsync();
        Assert.That(body, Does.Contain("Username not found"));
    }

    [Test]
    public async Task Register_For_Existing_User_Is_Bad_Request()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await _httpClient.PostAsJsonAsync(AuthController.RegisterRoute,
            new AuthRegisterDto
            {
                Email = user.Email,
                Password = "password"
            });
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}