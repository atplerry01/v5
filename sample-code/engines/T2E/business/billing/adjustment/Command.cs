namespace Whycespace.Engines.T2E.Business.Billing.Adjustment;

public record AdjustmentCommand(
    string Action,
    string EntityId,
    object Payload
);
