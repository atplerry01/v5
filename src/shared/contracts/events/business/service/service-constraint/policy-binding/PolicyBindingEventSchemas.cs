namespace Whycespace.Shared.Contracts.Events.Business.Service.ServiceConstraint.PolicyBinding;

public sealed record PolicyBindingCreatedEventSchema(
    Guid AggregateId,
    Guid ServiceDefinitionId,
    string PolicyRef,
    int Scope);

public sealed record PolicyBindingBoundEventSchema(Guid AggregateId, DateTimeOffset BoundAt);

public sealed record PolicyBindingUnboundEventSchema(Guid AggregateId, DateTimeOffset UnboundAt);

public sealed record PolicyBindingArchivedEventSchema(Guid AggregateId);
