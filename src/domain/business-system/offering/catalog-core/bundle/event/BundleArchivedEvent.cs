using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.Bundle;

public sealed record BundleArchivedEvent(
    [property: JsonPropertyName("AggregateId")] BundleId BundleId) : DomainEvent;
