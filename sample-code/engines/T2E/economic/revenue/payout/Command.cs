namespace Whycespace.Engines.T2E.Economic.Revenue.Payout;

public record PayoutCommand(string Action, string EntityId, object Payload);

public sealed record InitiatePayoutCommand(string Id, string WalletId, decimal Amount, string CurrencyCode, string RecipientId)
    : PayoutCommand("Initiate", Id, null!);

public sealed record CompletePayoutCommand(string PayoutId)
    : PayoutCommand("Complete", PayoutId, null!);

public sealed record CancelPayoutCommand(string PayoutId, string Reason)
    : PayoutCommand("Cancel", PayoutId, null!);
