using Application.Interfaces.Infrastructure.Postgres;
using Application.Models;
using Infrastructure.Postgres.Postgresql.Data;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Infrastructure.Postgres;

public static class Extensions
{
    public static IServiceCollection AddDataSourceAndRepositories(this IServiceCollection services)
    {
        
        
        services.AddDbContext<MyDbContext>((serviceProvider, options) =>
        {
            var appOpts = serviceProvider.GetRequiredService<IOptionsMonitor<AppOptions>>().CurrentValue;

            if (appOpts.IsTesting)
            {
                options.UseInMemoryDatabase("TestDb");
            }
            else
            {
                options.UseNpgsql(appOpts.DbConnectionString);
            }
            options.EnableSensitiveDataLogging();
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IGreenhouseDeviceRepository, GreenhouseDeviceRepository>();
        services.AddScoped<IAlertRepository, AlertRepository>();
        services.AddScoped<IEmailListRepository, EmailListRepository>();
        services.AddScoped<IPlantRepository, PlantRepository>();
        services.AddScoped<IUserDeviceRepository, UserDeviceRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        services.AddScoped<IAlertConditionRepository, AlertConditionRepository>();
        services.AddScoped<Seeder>();

        return services;
    }
}