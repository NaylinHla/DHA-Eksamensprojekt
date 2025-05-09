using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IAlertService
{
    Task<Alert> CreateAlertAsync(Guid userId, string title, string description, Guid? plantId = null);
    Task<List<Alert>> GetAlertsAsync(Guid userId, int? year = null);
}