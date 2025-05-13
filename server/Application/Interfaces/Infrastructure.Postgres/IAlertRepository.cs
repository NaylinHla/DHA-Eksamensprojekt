using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IAlertRepository
{
    Task<Alert> AddAlertAsync(Alert alert);
    Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null);
}