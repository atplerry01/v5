namespace Whycespace.Shared.Contracts.Economic.Transaction.Wallet;

public sealed record WalletReadModel
{
    public Guid WalletId { get; init; }
    public Guid OwnerId { get; init; }
    public Guid AccountId { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastTransactionRequestedAt { get; init; }
}
