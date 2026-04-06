namespace Whycespace.Engines.T2E.Decision.Audit.Remediation;

public record RemediationCommand(
    string Action,
    string EntityId,
    object Payload
);
