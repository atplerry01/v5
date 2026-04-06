namespace Whycespace.Projections.Economic.Transaction.Transaction;

public sealed record TransactionView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
