using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class Extensions
{
    public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<ISecurityService, SecurityService>();
        services.AddScoped<IGreenhouseDeviceService, GreenhouseDeviceService>();
        services.AddScoped<IWebsocketSubscriptionService, WebsocketSubscriptionService>();
        services.AddScoped<IAlertService, AlertService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPlantService, PlantService>();
        services.AddScoped<IUserDeviceService, UserDeviceService>();
        services.AddScoped<IUserSettingsService, UserSettingsService>();
        services.AddScoped<IAlertConditionService, AlertConditionService>();
        return services;
    }
}