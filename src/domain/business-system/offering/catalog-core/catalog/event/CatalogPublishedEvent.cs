using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Catalog;

public sealed record CatalogPublishedEvent(
    [property: JsonPropertyName("AggregateId")] CatalogId CatalogId) : DomainEvent;
