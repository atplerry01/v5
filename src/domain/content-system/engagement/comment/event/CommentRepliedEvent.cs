using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed record CommentRepliedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CommentId CommentId, CommentId ReplyToId, Timestamp RepliedAt) : DomainEvent;
