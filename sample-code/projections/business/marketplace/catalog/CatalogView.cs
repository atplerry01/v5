namespace Whycespace.Projections.Business.Marketplace.Catalog;

public sealed record CatalogView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
