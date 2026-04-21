namespace Whycespace.Shared.Contracts.Events.Business.Offering.CatalogCore.Product;

public sealed record ProductCreatedEventSchema(
    Guid AggregateId,
    string Name,
    string Type,
    Guid? CatalogId);

public sealed record ProductUpdatedEventSchema(
    Guid AggregateId,
    string Name,
    string Type);

public sealed record ProductActivatedEventSchema(Guid AggregateId);

public sealed record ProductArchivedEventSchema(Guid AggregateId);
