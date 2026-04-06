namespace Whycespace.Engines.T2E.Business.Billing.Statement;

public record StatementCommand(
    string Action,
    string EntityId,
    object Payload
);
