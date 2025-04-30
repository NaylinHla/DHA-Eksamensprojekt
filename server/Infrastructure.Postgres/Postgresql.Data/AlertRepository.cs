using Application.Interfaces.Infrastructure.Postgres;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Postgres.Postgresql.Data;

public class AlertRepository(MyDbContext ctx) : IAlertRepository
{
    public async Task<Alert> AddAlertAsync(Alert alert)
    {
        ctx.Alerts.Add(alert);
        await ctx.SaveChangesAsync();
        return alert;
    }

    public async Task<List<Alert>> GetAlertsAsync(Guid userId, int? year = null)
    {
        var query = ctx.Alerts.Where(a => a.AlertUserId == userId);

        if (year.HasValue)
            query = query.Where(a => a.AlertTime.Year == year.Value);

        return await query.OrderByDescending(a => a.AlertTime).ToListAsync();
    }
}
