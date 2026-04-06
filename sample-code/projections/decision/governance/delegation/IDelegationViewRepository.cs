namespace Whycespace.Projections.Decision.Governance.Delegation;

public interface IDelegationViewRepository
{
    Task SaveAsync(DelegationReadModel model, CancellationToken ct = default);
    Task<DelegationReadModel?> GetAsync(string id, CancellationToken ct = default);
}
