namespace Whycespace.Projections.Decision.Governance.GovernanceRecord;

public interface IGovernanceRecordViewRepository
{
    Task SaveAsync(GovernanceRecordReadModel model, CancellationToken ct = default);
    Task<GovernanceRecordReadModel?> GetAsync(string id, CancellationToken ct = default);
}
