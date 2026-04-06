namespace Whycespace.Engines.T2E.Decision.Audit.EvidenceAudit;

public record EvidenceAuditCommand(
    string Action,
    string EntityId,
    object Payload
);
