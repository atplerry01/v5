namespace Whycespace.Projections.Structural.Humancapital.Governance;

public interface IGovernanceViewRepository
{
    Task SaveAsync(GovernanceReadModel model, CancellationToken ct = default);
    Task<GovernanceReadModel?> GetAsync(string id, CancellationToken ct = default);
}
