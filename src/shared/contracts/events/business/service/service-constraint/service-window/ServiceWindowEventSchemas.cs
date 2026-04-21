namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceWindow;

public sealed record ServiceWindowCreatedEventSchema(
    Guid AggregateId,
    Guid ServiceDefinitionId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record ServiceWindowUpdatedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record ServiceWindowActivatedEventSchema(Guid AggregateId);

public sealed record ServiceWindowArchivedEventSchema(Guid AggregateId);
