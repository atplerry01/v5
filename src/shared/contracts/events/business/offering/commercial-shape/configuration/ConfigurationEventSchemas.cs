namespace Whycespace.Shared.Contracts.Events.Business.Offering.CommercialShape.Configuration;

public sealed record ConfigurationCreatedEventSchema(Guid AggregateId, string Name);

public sealed record ConfigurationOptionSetEventSchema(Guid AggregateId, string Key, string Value);

public sealed record ConfigurationOptionRemovedEventSchema(Guid AggregateId, string Key);

public sealed record ConfigurationActivatedEventSchema(Guid AggregateId);

public sealed record ConfigurationArchivedEventSchema(Guid AggregateId);
