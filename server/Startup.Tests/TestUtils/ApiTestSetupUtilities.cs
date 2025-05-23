﻿using System.Net.Http.Json;
using System.Security.Cryptography;
using Api.Rest.Controllers;
using Application;
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
        bool useTestContainer = false,
        bool useInMemory = true,
        bool mockProxyConfig = true,
        bool makeWsClient = true,
        bool makeMqttClient = false,
        Action? customSeeder = null
    )
    {
        var jwtSecretBytes = RandomNumberGenerator.GetBytes(32); // 256 bits
        var jwtSecret = Convert.ToBase64String(jwtSecretBytes);

        services.Configure<AppOptions>(options =>
        {
            options.JwtSecret = jwtSecret;
            options.Seed = true;
            options.DbConnectionString = "testDBConnectionString";
            options.PORT = 8080;
            options.WS_PORT = 8181;
            options.REST_PORT = 5000;
            options.IsTesting = true;
        });

        RemoveExistingService<DbContextOptions<MyDbContext>>(services);
        
        if (useInMemory)
        {
            services.AddDbContext<MyDbContext>(opt => 
                opt.UseInMemoryDatabase("testDB"));
        }
        else if (useTestContainer)
        {
            var db = new PgCtxSetup<MyDbContext>();
            services.AddDbContext<MyDbContext>(opt =>
            {
                opt.UseNpgsql(db._postgres.GetConnectionString());
                opt.EnableSensitiveDataLogging();
                opt.LogTo(_ => { });
            });
        }
        else
        {
            throw new ArgumentException("Must choose either TestContainer or InMemory");
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
        var random = new Random().Next(100000, 999999);
        var email = $"test{random}@gmail.com";
        var password = $"Pass{random}!";

        var registerDto = new AuthRegisterDto
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = password,
            Country = "TestCountry",
            Birthday = DateTime.UtcNow.AddYears(-25) // Required and UTC
        };

        var signIn = await httpClient.PostAsJsonAsync(AuthController.RegisterRoute, registerDto);

        if (!signIn.IsSuccessStatusCode)
        {
            var error = await signIn.Content.ReadAsStringAsync();
            throw new Exception($"Registration failed: {signIn.StatusCode} - {error}");
        }

        var authResponseDto = await signIn.Content.ReadFromJsonAsync<AuthResponseDto>(JsonDefaults.CaseInsensitive) ??
                              throw new Exception("Failed to deserialize AuthResponseDto");

        httpClient.DefaultRequestHeaders.Add("authorization", authResponseDto.Jwt);
        return authResponseDto;
    }
}