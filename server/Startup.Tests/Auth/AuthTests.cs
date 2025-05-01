using System.Net;
using System.Net.Http.Json;
using Api.Rest.Controllers;
using Application.Models.Dtos.RestDtos;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Startup.Tests.TestUtils;

namespace Startup.Tests.Auth;

public class AuthTests : WebApplicationFactory<Program>
{
    private HttpClient _httpClient;

    [SetUp]
    public void Setup()
    {
        _httpClient = CreateClient();
    }


    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => { services.DefaultTestConfig(makeMqttClient: false); });
    }


    [Test]
    public async Task RouteWithNoAuth_Can_Be_Accessed()
    {
        var client = CreateClient();
        var response = await client.GetAsync("/acceptance");
        if (await response.Content.ReadAsStringAsync() != "Accepted")
            throw new Exception("Expected 'Accepted'");
        if (HttpStatusCode.OK != response.StatusCode)
            throw new Exception("Expected OK status code");
    }

    [Test]
    public async Task SecuredRouteIsBlockedWitoutJwt()
    {
        var response = await CreateClient().GetAsync(AuthController.SecuredRoute);
        if (HttpStatusCode.Unauthorized != response.StatusCode)
            throw new Exception("Expected Unauthorized status code");
    }

    [Test]
    public async Task Register_Can_Register_And_Return_Jwt()
    {
        var user = MockObjects.GetUser();
        var response = await _httpClient.PostAsJsonAsync(AuthController.RegisterRoute,
            new AuthRegisterDto
            {
                Email = user.Email,
                Password = "pass",
                FirstName = user.FirstName,
                LastName = user.LastName,
                Country = user.Country,
                Birthday = user.Birthday ?? DateTime.UtcNow.AddYears(-30)
            });
        if (HttpStatusCode.OK != response.StatusCode)
            throw new Exception("Expected OK status code");
        if ((await response.Content.ReadFromJsonAsync<AuthResponseDto>())!.Jwt.Length < 10)
            throw new Exception("Expected jwt to be longer than 10 characters");
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
        if (HttpStatusCode.BadRequest != response.StatusCode)
            throw new Exception("Expected BadRequest status code");
    }

    [Test]
    public async Task Login_Can_Login_And_Return_Jwt()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await _httpClient.PostAsJsonAsync(
            AuthController.LoginRoute, new AuthLoginDto
            {
                Email = user.Email,
                Password = "pass"
            });
        if (HttpStatusCode.OK != response.StatusCode)
            throw new Exception("Expected OK status code");
    }

    [Test]
    public async Task Invalid_Login_Gives_Unauthorized()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await CreateClient().PostAsJsonAsync(AuthController.LoginRoute,
            new AuthLoginDto
            {
                Email = user.Email,
                Password = "invalid password"
            });
        if (HttpStatusCode.Unauthorized != response.StatusCode)
            throw new Exception("Expected Unauthorized status code");
    }

    [Test]
    public async Task Login_For_Non_Existing_User_Is_Unauthorized()
    {
        var response = await CreateClient().PostAsJsonAsync(AuthController.LoginRoute,
            new AuthLoginDto { Email = "bob@bob.dk", Password = "password" });
        if (HttpStatusCode.BadRequest != response.StatusCode)
            throw new Exception("Expected BadRequest status code");
    }

    [Test]
    public async Task Register_For_Existing_User_Is_Bad_Request()
    {
        using var scope = Services.CreateScope();
        var ctx = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        var user = MockObjects.GetUser();
        ctx.Users.Add(user);
        await ctx.SaveChangesAsync();

        var response = await CreateClient().PostAsJsonAsync(AuthController.RegisterRoute,
            new AuthRegisterDto
            {
                Email = user.Email,
                Password = "password"
            });
        if (HttpStatusCode.BadRequest != response.StatusCode)
            throw new Exception("Expected BadRequest status code");
    }
}