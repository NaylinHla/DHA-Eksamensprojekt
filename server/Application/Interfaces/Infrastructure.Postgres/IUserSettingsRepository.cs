using Core.Domain.Entities;

namespace Application.Interfaces.Infrastructure.Postgres;

public interface IUserSettingsRepository
{
    void Add(UserSettings settings);
    void Update(UserSettings settings);
    UserSettings? GetByUserId(Guid userId);
}
