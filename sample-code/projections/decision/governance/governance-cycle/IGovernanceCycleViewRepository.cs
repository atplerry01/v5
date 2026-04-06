namespace Whycespace.Projections.Decision.Governance.GovernanceCycle;

public interface IGovernanceCycleViewRepository
{
    Task SaveAsync(GovernanceCycleReadModel model, CancellationToken ct = default);
    Task<GovernanceCycleReadModel?> GetAsync(string id, CancellationToken ct = default);
}
