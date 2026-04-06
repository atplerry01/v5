namespace Whycespace.Projections.Business.Marketplace.ParticipantMarket;

public sealed record ParticipantMarketView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
