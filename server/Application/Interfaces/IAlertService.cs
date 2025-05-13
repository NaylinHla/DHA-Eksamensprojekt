using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;

namespace Application.Interfaces;

public interface IAlertService
{
    Task<Alert> CreateAlertAsync(Guid userId, AlertCreateDto dto);
    Task<List<AlertResponseDto>> GetAlertsAsync(Guid userId, int? year = null);
}