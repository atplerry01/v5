using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingCreatedEvent(
    ServiceOfferingId ServiceOfferingId,
    ServiceOfferingName Name,
    ServiceDefinitionRef ServiceDefinition,
    OfferingPackageRef? Package);
