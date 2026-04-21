namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.ServiceConstraint;

public sealed record ServiceConstraintCreatedEventSchema(
    Guid AggregateId,
    Guid ServiceDefinitionId,
    int Kind,
    string Descriptor);

public sealed record ServiceConstraintUpdatedEventSchema(
    Guid AggregateId,
    int Kind,
    string Descriptor);

public sealed record ServiceConstraintActivatedEventSchema(Guid AggregateId);

public sealed record ServiceConstraintArchivedEventSchema(Guid AggregateId);
