namespace Whycespace.Projections.Economic.Ledger.Settlement;

public sealed record SettlementView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
