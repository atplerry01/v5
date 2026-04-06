namespace Whycespace.Engines.T2E.Economic.Transaction.Wallet;

public record WalletResult(bool Success, string Message);

public sealed record WalletDto(string WalletId, string IdentityId, string CurrencyCode, string Status);
