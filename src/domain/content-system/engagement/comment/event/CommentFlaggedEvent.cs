using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed record CommentFlaggedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CommentId CommentId, string ReporterRef, string Reason, Timestamp FlaggedAt) : DomainEvent;
