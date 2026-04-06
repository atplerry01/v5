namespace Whycespace.Engines.T2E.Decision.Compliance.ComplianceCase;

public record ComplianceCaseCommand(
    string Action,
    string EntityId,
    object Payload
);
