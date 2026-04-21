using System.Text.Json.Serialization;
using Whycespace.Domain.BusinessSystem.Shared.Reference;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ServiceOfferingId ServiceOfferingId,
    ServiceOfferingName Name,
    ServiceDefinitionRef ServiceDefinition,
    OfferingPackageRef? Package) : DomainEvent;
