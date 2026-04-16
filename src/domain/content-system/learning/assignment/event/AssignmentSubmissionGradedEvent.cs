using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed record AssignmentSubmissionGradedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssignmentId AssignmentId, Guid SubmissionId, decimal Grade, decimal MaxGrade, Timestamp GradedAt) : DomainEvent;
