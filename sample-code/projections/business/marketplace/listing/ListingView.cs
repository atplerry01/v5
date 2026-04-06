namespace Whycespace.Projections.Business.Marketplace.Listing;

public sealed record ListingView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
