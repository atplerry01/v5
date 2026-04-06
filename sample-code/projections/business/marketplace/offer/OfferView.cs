namespace Whycespace.Projections.Business.Marketplace.Offer;

public sealed record OfferView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
