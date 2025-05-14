using Application.Interfaces.Infrastructure.Postgres;
using Application.Models.Dtos.RestDtos;
using Core.Domain.Entities;
using Infrastructure.Postgres.Scaffolding;

namespace Infrastructure.Postgres.Postgresql.Data;

public class AlertConditionRepository(
    MyDbContext ctx,
    IPlantRepository plantRepository,
    IUserDeviceRepository userDeviceRepository) : IAlertConditionRepository
{
    public Task<ConditionAlertPlant?> GetConditionAlertPlantByIdAsync(Guid conditionAlertPlantId)
    {
        throw new NotImplementedException();
    }

    public Task<List<ConditionAlertPlant>> GetAllConditionAlertPlantsAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertPlant> AddConditionAlertPlantAsync(Guid plantId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConditionAlertPlantAsync(Guid conditionAlertPlantId)
    {
        throw new NotImplementedException();
    }


    public Task<ConditionAlertUserDevice?> GetConditionAlertUserDeviceByIdAsync(Guid conditionAlertUserDeviceId)
    {
        throw new NotImplementedException();
    }

    public Task<List<ConditionAlertUserDevice>> GetAllConditionAlertUserDevicesAsync(Guid userId)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertUserDevice> AddConditionAlertUserDeviceAsync(ConditionAlertUserDeviceCreateDto dto)
    {
        throw new NotImplementedException();
    }

    public Task<ConditionAlertUserDevice> EditConditionAlertUserDeviceAsync(ConditionAlertUserDeviceEditDto dto)
    {
        throw new NotImplementedException();
    }

    public Task DeleteConditionAlertUserDeviceAsync(Guid conditionAlertUserDeviceId)
    {
        throw new NotImplementedException();
    }

    public Task SaveChangesAsync()
    {
        throw new NotImplementedException();
    }
}