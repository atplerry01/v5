namespace Whycespace.Engines.T2E.Economic.Transaction.Charge;

public record ChargeCommand(string Action, string EntityId, object Payload);

public sealed record CreateChargeCommand(string Id, string WalletId, decimal Amount, string CurrencyCode, string Description)
    : ChargeCommand("Create", Id, null!);

public sealed record ApproveChargeCommand(string ChargeId)
    : ChargeCommand("Approve", ChargeId, null!);

public sealed record RejectChargeCommand(string ChargeId, string Reason)
    : ChargeCommand("Reject", ChargeId, null!);
