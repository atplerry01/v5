namespace Whycespace.Engines.T2E.Decision.Audit.AuditLog;

public record AuditLogCommand(
    string Action,
    string EntityId,
    object Payload
);
