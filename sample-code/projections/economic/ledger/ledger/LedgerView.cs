namespace Whycespace.Projections.Economic.Ledger.Ledger;

public sealed record LedgerView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
