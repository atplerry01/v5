using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Learning.Assignment;

public sealed record AssignmentSubmissionReceivedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    AssignmentId AssignmentId, Guid SubmissionId, string LearnerRef, string ContentRef, Timestamp ReceivedAt) : DomainEvent;
