using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOfferingId ServiceOfferingId,
    ServiceOfferingName Name) : DomainEvent;
