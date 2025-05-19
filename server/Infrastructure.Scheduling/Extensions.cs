using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Scheduling;

public static class Extensions
{
    public static IServiceCollection AddScheduledInfrastructure(this IServiceCollection services)
    {
        services.AddHostedService<DailyTaskScheduler>();
        return services;
    }
}
