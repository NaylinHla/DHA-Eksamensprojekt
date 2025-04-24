using System.Net.Http.Json;
using System.Text.Json;
using Api.Rest.Controllers;
using Application.Models;
using Application.Models.Dtos.RestDtos;
using Infrastructure.Postgres;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using PgCtx;
using Startup.Proxy;

namespace Startup.Tests.TestUtils;

public static class ApiTestSetupUtilities
{
    public static IServiceCollection DefaultTestConfig(
        this IServiceCollection services,
        bool useTestContainer = true,
        bool mockProxyConfig = true,
        bool makeWsClient = true,
        bool makeMqttClient = true,
        Action? customSeeder = null
    )
    {
        if (useTestContainer)
        {
            var db = new PgCtxSetup<MyDbContext>();
            RemoveExistingService<DbContextOptions<MyDbContext>>(services);
            services.AddDbContext<MyDbContext>(opt =>
            {
                opt.UseNpgsql(db._postgres.GetConnectionString());
                opt.EnableSensitiveDataLogging();
                opt.LogTo(_ => { });
            });
        }

        if (mockProxyConfig)
        {
            RemoveExistingService<IProxyConfig>(services);
            var mockProxy = new Mock<IProxyConfig>();
            services.AddSingleton(mockProxy.Object);
        }

        if (customSeeder is not null)
        {
            RemoveExistingService<ISeeder>(services);
            customSeeder.Invoke();
        }

        if (makeWsClient) services.AddScoped<TestWsClient>();
        if (makeMqttClient)
        {
            RemoveExistingService<TestMqttClient>(services);
            services.AddScoped<TestMqttClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptionsMonitor<AppOptions>>().CurrentValue;
                return new TestMqttClient(options.MQTT_BROKER_HOST, options.MQTT_USERNAME);
            });
        }

        return services;
    }

    private static void RemoveExistingService<T>(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(T));
        if (descriptor != null)
            services.Remove(descriptor);
    }

    public static async Task<AuthResponseDto> TestRegisterAndAddJwt(HttpClient httpClient)
    {
        var registerDto = new AuthRequestDto
        {
            Email = new Random().NextDouble() * 123 + "@gmail.com",
            Password = new Random().NextDouble() * 123 + "@gmail.com"
        };
        var signIn = await httpClient.PostAsJsonAsync(
            AuthController.RegisterRoute, registerDto);
        var authResponseDto = await signIn.Content
                                  .ReadFromJsonAsync<AuthResponseDto>(new JsonSerializerOptions
                                      { PropertyNameCaseInsensitive = true }) ??
                              throw new Exception("Failed to deserialize " + await signIn.Content.ReadAsStringAsync() +
                                                  " to " + nameof(AuthResponseDto));
        httpClient.DefaultRequestHeaders.Add("Authorization", authResponseDto.Jwt);
        return authResponseDto;
    }
}