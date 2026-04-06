namespace Whycespace.Projections.Business.Marketplace.ParticipantMarket;

public sealed record ParticipantMarketReadModel
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
    public DateTimeOffset LastEventTimestamp { get; init; }
    public long LastEventVersion { get; init; }
}
