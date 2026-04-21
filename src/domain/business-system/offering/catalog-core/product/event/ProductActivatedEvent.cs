using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Product;

public sealed record ProductActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ProductId ProductId) : DomainEvent;
