namespace Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.ServiceOffering;

public sealed record ServiceOfferingCreatedEventSchema(
    Guid AggregateId,
    string Name,
    Guid ServiceDefinitionId,
    Guid? PackageId);

public sealed record ServiceOfferingUpdatedEventSchema(
    Guid AggregateId,
    string Name);

public sealed record ServiceOfferingActivatedEventSchema(Guid AggregateId);

public sealed record ServiceOfferingArchivedEventSchema(Guid AggregateId);
