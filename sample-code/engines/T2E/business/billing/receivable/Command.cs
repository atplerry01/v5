namespace Whycespace.Engines.T2E.Business.Billing.Receivable;

public record ReceivableCommand(
    string Action,
    string EntityId,
    object Payload
);
