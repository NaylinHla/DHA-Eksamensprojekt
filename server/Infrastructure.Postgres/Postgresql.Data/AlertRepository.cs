using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Logging;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class AlertRepository(MyDbContext ctx) : IAlertRepository
{
    public async Task<Alert> AddAlertAsync(Alert alert)
    {
        MonitorService.Log.Debug("Entered AddAlertAsync method in AlertRepository");

        ctx.Alerts.Add(alert);
        await ctx.SaveChangesAsync();

        MonitorService.Log.Debug("Successfully added alert with ID: " + alert.AlertId);
        return alert;
    }

    public async Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null)
    {
        MonitorService.Log.Debug("Entered GetAlertsAsync method in AlertRepository for userId: " + userId + " and year: " + (year.HasValue ? year.Value.ToString() : "null"));

        var query = ctx.Alerts
            .Where(a => a.AlertUserId == userId);

        if (year.HasValue)
        {
            MonitorService.Log.Debug("Filtering alerts by year: " + year.Value);
            query = query.Where(a => a.AlertTime.Year == year.Value);
        }

        var alerts = await query
            .OrderByDescending(a => a.AlertTime)
            .Select(a => new AlertResponseDto
            {
                AlertId = a.AlertId,
                AlertName = a.AlertName,
                AlertDesc = a.AlertDesc,
                AlertTime = a.AlertTime,
                AlertPlantConditionId = a.AlertPlantConditionId,
                AlertDeviceConditionId = a.AlertDeviceConditionId
            })
            .ToListAsync();

        MonitorService.Log.Debug("Fetched " + alerts.Count + " alerts for userId: " + userId);
        return alerts;
    }
}