namespace Whycespace.Projections.Decision.Governance.Scope;

public interface IScopeViewRepository
{
    Task SaveAsync(ScopeReadModel model, CancellationToken ct = default);
    Task<ScopeReadModel?> GetAsync(string id, CancellationToken ct = default);
}
