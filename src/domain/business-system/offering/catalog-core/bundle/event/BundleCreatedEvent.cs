using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed record BundleCreatedEvent(
    [property: JsonPropertyName("AggregateId")] BundleId BundleId,
    BundleName Name) : DomainEvent;
