namespace Whycespace.Engines.T2E.Business.Agreement.Obligation;

public record ObligationCommand(
    string Action,
    string EntityId,
    object Payload
);
