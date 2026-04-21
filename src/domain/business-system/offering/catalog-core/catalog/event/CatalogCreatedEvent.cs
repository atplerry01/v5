using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed record CatalogCreatedEvent(
    [property: JsonPropertyName("AggregateId")] CatalogId CatalogId,
    CatalogStructure Structure) : DomainEvent;
