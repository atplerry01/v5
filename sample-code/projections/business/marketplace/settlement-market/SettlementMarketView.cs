namespace Whycespace.Projections.Business.Marketplace.SettlementMarket;

public sealed record SettlementMarketView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
