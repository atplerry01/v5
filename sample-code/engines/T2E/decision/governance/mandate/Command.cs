namespace Whycespace.Engines.T2E.Decision.Governance.Mandate;

public record MandateCommand(
    string Action,
    string EntityId,
    object Payload
);
