using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed record BundleActivatedEvent(
    [property: JsonPropertyName("AggregateId")] BundleId BundleId) : DomainEvent;
