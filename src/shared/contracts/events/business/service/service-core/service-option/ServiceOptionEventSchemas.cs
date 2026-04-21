namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceCore.ServiceOption;

public sealed record ServiceOptionCreatedEventSchema(
    Guid AggregateId,
    Guid ServiceDefinitionId,
    string Code,
    string Name,
    string Kind);

public sealed record ServiceOptionUpdatedEventSchema(
    Guid AggregateId,
    string Name,
    string Kind);

public sealed record ServiceOptionActivatedEventSchema(Guid AggregateId);

public sealed record ServiceOptionArchivedEventSchema(Guid AggregateId);
