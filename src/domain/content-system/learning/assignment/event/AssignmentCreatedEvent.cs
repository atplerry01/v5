using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed record AssignmentCreatedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssignmentId AssignmentId, string CourseRef, string Title, decimal MaxGrade, Timestamp CreatedAt) : DomainEvent;
