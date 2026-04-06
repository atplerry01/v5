namespace Whycespace.Engines.T2E.Decision.Audit.AuditCase;

public record AuditCaseCommand(
    string Action,
    string EntityId,
    object Payload
);
