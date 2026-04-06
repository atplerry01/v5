namespace Whycespace.Projections.Decision.Audit.AuditLog;

public interface IAuditLogViewRepository
{
    Task SaveAsync(AuditLogReadModel model, CancellationToken ct = default);
    Task<AuditLogReadModel?> GetAsync(string id, CancellationToken ct = default);
}
