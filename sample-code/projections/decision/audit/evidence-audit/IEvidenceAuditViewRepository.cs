namespace Whycespace.Projections.Decision.Audit.EvidenceAudit;

public interface IEvidenceAuditViewRepository
{
    Task SaveAsync(EvidenceAuditReadModel model, CancellationToken ct = default);
    Task<EvidenceAuditReadModel?> GetAsync(string id, CancellationToken ct = default);
}
