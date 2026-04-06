namespace Whycespace.Engines.T2E.Business.Agreement.Acceptance;

public record AcceptanceCommand(
    string Action,
    string EntityId,
    object Payload
);
