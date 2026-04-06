namespace Whycespace.Projections.Decision.Governance.Sanction;

public interface ISanctionViewRepository
{
    Task SaveAsync(SanctionReadModel model, CancellationToken ct = default);
    Task<SanctionReadModel?> GetAsync(string id, CancellationToken ct = default);
}
