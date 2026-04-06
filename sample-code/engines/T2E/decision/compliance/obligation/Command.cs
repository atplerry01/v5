namespace Whycespace.Engines.T2E.Decision.Compliance.Obligation;

public record ObligationCommand(
    string Action,
    string EntityId,
    object Payload
);
