namespace Whycespace.Engines.T2E.Business.Billing.BillRun;

public record BillRunCommand(
    string Action,
    string EntityId,
    object Payload
);
