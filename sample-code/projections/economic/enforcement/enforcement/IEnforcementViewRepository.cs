namespace Whycespace.Projections.Economic.Enforcement.Enforcement;

public interface IEnforcementViewRepository
{
    Task SaveAsync(EnforcementReadModel model, CancellationToken ct = default);
    Task<EnforcementReadModel?> GetAsync(string id, CancellationToken ct = default);
}
