namespace Whycespace.Projections.Constitutional.Policy.Enforcement;

public interface IEnforcementViewRepository
{
    Task SaveAsync(EnforcementReadModel model, CancellationToken ct = default);
    Task<EnforcementReadModel?> GetAsync(string id, CancellationToken ct = default);
}
