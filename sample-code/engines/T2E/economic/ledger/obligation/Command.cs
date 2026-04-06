namespace Whycespace.Engines.T2E.Economic.Ledger.Obligation;

public record ObligationCommand(string Action, string EntityId, object Payload);

public sealed record CreateObligationCommand(string Id, string DebtorId, string CreditorId, decimal Amount, string CurrencyCode)
    : ObligationCommand("Create", Id, null!);

public sealed record ActivateObligationCommand(string ObligationId)
    : ObligationCommand("Activate", ObligationId, null!);

public sealed record SettleObligationCommand(string ObligationId)
    : ObligationCommand("Settle", ObligationId, null!);

public sealed record DefaultObligationCommand(string ObligationId, string Reason)
    : ObligationCommand("Default", ObligationId, null!);
