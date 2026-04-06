namespace Whycespace.Projections.Economic.Ledger.Treasury;

public sealed record TreasuryView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
