namespace Whycespace.Projections.CoreSystem.SystemHealth;

public interface ISystemHealthViewRepository
{
    Task SaveAsync(SystemHealthReadModel model, CancellationToken ct = default);
    Task<SystemHealthReadModel?> GetAsync(string id, CancellationToken ct = default);
}
