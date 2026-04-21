namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceDefinition;

public sealed record ServiceDefinitionCreatedEventSchema(Guid AggregateId, string Name, string Category);

public sealed record ServiceDefinitionUpdatedEventSchema(Guid AggregateId, string Name, string Category);

public sealed record ServiceDefinitionActivatedEventSchema(Guid AggregateId);

public sealed record ServiceDefinitionArchivedEventSchema(Guid AggregateId);
