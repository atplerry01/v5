using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed record AssignmentPublishedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssignmentId AssignmentId, Timestamp PublishedAt) : DomainEvent;
