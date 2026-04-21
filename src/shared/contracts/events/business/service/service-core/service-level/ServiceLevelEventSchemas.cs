namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceLevel;

public sealed record ServiceLevelCreatedEventSchema(
    Guid AggregateId,
    Guid ServiceDefinitionId,
    string Code,
    string Name,
    string Target);

public sealed record ServiceLevelUpdatedEventSchema(
    Guid AggregateId,
    string Name,
    string Target);

public sealed record ServiceLevelActivatedEventSchema(Guid AggregateId);

public sealed record ServiceLevelArchivedEventSchema(Guid AggregateId);
