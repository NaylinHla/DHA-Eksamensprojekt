using Application.Interfaces;
using Application.Interfaces.Infrastructure.Postgres;
using Application.Interfaces.Infrastructure.Websocket;
using Infrastructure.Postgres.Scaffolding;
using Application.Models;
using Application.Models.Dtos;
using Application.Models.Dtos.BroadcastModels;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Application.Services;

public class AlertService : IAlertService
{
    private readonly IServiceProvider _services;
    private readonly IConnectionManager _ws;
    private readonly ILogger<AlertService> _logger;

    public AlertService(IServiceProvider services, IConnectionManager ws, ILogger<AlertService> logger)
    {
        _services = services;
        _ws = ws;
        _logger = logger;
    }

    public async Task<Alert> CreateAlertAsync(Guid userId, string title, string description, Guid? plantId = null)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        var alert = new Alert
        {
            AlertID = Guid.NewGuid(),
            AlertUserId = userId,
            AlertName = title,
            AlertDesc = description,
            AlertTime = DateTime.UtcNow,
            AlertPlant = plantId
        };

        try
        {
            db.Alerts.Add(alert);
            await db.SaveChangesAsync();

            await _ws.BroadcastToTopic($"alerts-{userId}", new
            {
                type = "alert",
                data = new
                {
                    alert.AlertID,
                    alert.AlertName,
                    alert.AlertDesc,
                    alert.AlertTime,
                    alert.AlertPlant
                }
            });

            return alert;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating alert");
            throw;
        }
    }

    public async Task<List<Alert>> GetAlertsAsync(Guid userId, int? year = null)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();

        try
        {
            var query = db.Alerts
                .Where(a => a.AlertUserId == userId);

            if (year.HasValue)
            {
                query = query.Where(a => a.AlertTime.Year == year.Value);
            }

            return await query
                .OrderByDescending(a => a.AlertTime)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching alerts");
            throw;
        }
    }
}

