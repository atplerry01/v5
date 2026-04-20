namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed record ProductCreatedEvent(
    ProductId ProductId,
    ProductName Name,
    ProductType Type,
    CatalogRef? Catalog);
