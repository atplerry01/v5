namespace Whycespace.Projections.Economic.Transaction.Wallet;

public sealed record WalletView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
