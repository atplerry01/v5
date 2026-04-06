namespace Whycespace.Projections.Intelligence.Observability.Health;

public interface IHealthViewRepository
{
    Task SaveAsync(HealthReadModel model, CancellationToken ct = default);
    Task<HealthReadModel?> GetAsync(string id, CancellationToken ct = default);
}
