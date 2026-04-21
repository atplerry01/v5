using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed record ProductCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProductId ProductId,
    ProductName Name,
    ProductType Type,
    CatalogRef? Catalog) : DomainEvent;
