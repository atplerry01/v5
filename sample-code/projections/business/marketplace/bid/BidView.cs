namespace Whycespace.Projections.Business.Marketplace.Bid;

public sealed record BidView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
