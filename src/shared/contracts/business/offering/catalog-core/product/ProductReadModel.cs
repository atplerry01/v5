namespace Whycespace.Shared.Contracts.Business.Offering.CatalogCore.Product;

public sealed record ProductReadModel
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public Guid? CatalogId { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset LastUpdatedAt { get; init; }
}
