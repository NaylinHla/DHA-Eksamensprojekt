using Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Infrastructure.Scheduling
{
    public class DailyTaskScheduler(IServiceScopeFactory scopeFactory) : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.UtcNow;
                var target = now.Date.AddHours(6);
                if (now >= target)
                    target = target.AddDays(1);

                var delay = target - now;

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }

                if (stoppingToken.IsCancellationRequested)
                    break;

                using var scope = scopeFactory.CreateScope();
                var svc = scope.ServiceProvider.GetRequiredService<IAlertService>();

                try
                {
                    await svc.CheckAndTriggerScheduledPlantAlertsAsync();
                }
                catch
                {
                    // Silently swallow exceptions if logging is removed
                }
            }
        }
    }
}