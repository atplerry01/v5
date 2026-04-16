using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed record CommentPostedEvent(
    EventId EventId, AggregateId AggregateId, CorrelationId CorrelationId, CausationId CausationId,
    CommentId CommentId, string TargetRef, string AuthorRef, string Body, Timestamp PostedAt) : DomainEvent;
