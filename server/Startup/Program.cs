using System.Text.Json;
using Api.Rest;
using Api.Websocket;
using Application;
using Application.Interfaces;
using Application.Interfaces.Infrastructure.MQTT;
using Application.Models;
using Application.Services;
using Infrastructure.MQTT;
using Infrastructure.Postgres;
using Infrastructure.Scheduling;
using Infrastructure.Websocket;
using Microsoft.Extensions.Options;
using NSwag.Generation;
using Scalar.AspNetCore;
using Startup.Documentation;
using Startup.Proxy;
using FeatureHubSDK;

namespace Startup;

public class Program
{
    public static async Task Main()
    {
        var builder = WebApplication.CreateBuilder();
        ConfigureServices(builder.Services, builder.Configuration);
        var app = builder.Build();
        await ConfigureMiddleware(app);
        await app.RunAsync();
    }

    private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Load AppOptions
        services.Configure<AppOptions>(configuration.GetSection("AppOptions"));

        // FeatureHub setup (v2.5.1)
        var apiKey = configuration["FeatureHub:ApiKey"];
        var edgeUrl = configuration["FeatureHub:EdgeUrl"];

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(edgeUrl))
            throw new FeatureHubKeyInvalidException("Edge URL or SDK key is missing from configuration.");

        var featureHubConfig = new EdgeFeatureHubConfig(edgeUrl, apiKey);
        services.AddSingleton(featureHubConfig);
        services.AddSingleton<IClientContext>(sp =>
        {
            var context = featureHubConfig.NewContext();
            context.Build().Wait();
            return context;
        });
        
        // Application core services
        services.RegisterApplicationServices();
        services.AddScheduledInfrastructure();
        services.AddDataSourceAndRepositories();

        // Email-related services
        services.AddTransient<IEmailSender, EmailSenderService>();
        services.AddSingleton<JwtEmailTokenService>();

        // Websocket & REST APIs
        services.AddWebsocketInfrastructure();
        services.RegisterWebsocketApiServices();
        services.RegisterRestApiServices();

        // AppOptions override
        var appOptions = configuration.GetSection("AppOptions").Get<AppOptions>();
        services.Configure<AppOptions>(options => { options.EnableEmailSending = false; });

        // MQTT setup or fallback
        if (!string.IsNullOrEmpty(appOptions?.MQTT_BROKER_HOST))
        {
            services.RegisterMqttInfrastructure();
        }
        else
        {
            Console.WriteLine("Skipping MQTT service registration: Using mock publisher");
            services.AddSingleton<IMqttPublisher, MockMqttPublisher>();
        }

        // OpenAPI & TypeScript generation
        services.AddOpenApiDocument(conf =>
        {
            conf.DocumentProcessors.Add(new AddAllDerivedTypesProcessor());
            conf.DocumentProcessors.Add(new AddStringConstantsProcessor());
        });

        services.AddSingleton<IProxyConfig, ProxyConfig>();
    }

    private static async Task ConfigureMiddleware(WebApplication app)
    {
        var logger = app.Services.GetRequiredService<ILogger<IOptionsMonitor<AppOptions>>>();
        var appOptions = app.Services.GetRequiredService<IOptionsMonitor<AppOptions>>().CurrentValue;
        var serializedAppOptions = JsonSerializer.Serialize(appOptions);
        logger.LogInformation("Serialized AppOptions: {serializedAppOptions}", serializedAppOptions);

        using (var scope = app.Services.CreateScope())
        {
            if (appOptions.Seed)
                await scope.ServiceProvider.GetRequiredService<Seeder>().Seed();
        }

        app.Urls.Clear();
        app.Urls.Add($"http://0.0.0.0:{appOptions.REST_PORT}");
        app.Services.GetRequiredService<IProxyConfig>()
            .StartProxyServer(appOptions.PORT, appOptions.REST_PORT, appOptions.WS_PORT);

        app.ConfigureRestApi();

        if (!appOptions.IsTesting && !string.IsNullOrEmpty(appOptions.MQTT_BROKER_HOST))
            await app.ConfigureMqtt();
        else
            Console.WriteLine("Skipping MQTT service configuration");

        await app.ConfigureWebsocketApi(appOptions.WS_PORT);

        app.MapGet("Acceptance", () => "Accepted");

        app.UseOpenApi(conf => { conf.Path = "openapi/v1.json"; });

        var document = await app.Services.GetRequiredService<IOpenApiDocumentGenerator>().GenerateAsync("v1");
        var json = document.ToJson();
        await File.WriteAllTextAsync("openapi.json", json);

        app.GenerateTypeScriptClient("/../../client/src/generated-client.ts").GetAwaiter().GetResult();
        app.MapScalarApiReference();
    }
}
