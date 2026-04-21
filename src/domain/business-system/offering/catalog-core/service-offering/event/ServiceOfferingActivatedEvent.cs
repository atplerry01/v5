using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOfferingId ServiceOfferingId) : DomainEvent;
