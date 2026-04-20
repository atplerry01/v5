namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed record ProductUpdatedEvent(ProductId ProductId, ProductName Name, ProductType Type);
