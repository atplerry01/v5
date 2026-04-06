namespace Whycespace.Projections.Decision.Audit.AuditCase;

public interface IAuditCaseViewRepository
{
    Task SaveAsync(AuditCaseReadModel model, CancellationToken ct = default);
    Task<AuditCaseReadModel?> GetAsync(string id, CancellationToken ct = default);
}
