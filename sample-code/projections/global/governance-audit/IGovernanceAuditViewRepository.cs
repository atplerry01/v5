namespace Whycespace.Projections.Global.GovernanceAudit;

public interface IGovernanceAuditViewRepository
{
    Task SaveAsync(GovernanceAuditReadModel model, CancellationToken ct = default);
    Task<GovernanceAuditReadModel?> GetAsync(string id, CancellationToken ct = default);
    Task<IReadOnlyList<GovernanceAuditReadModel>> GetRecentAsync(int count, CancellationToken ct = default);
}
