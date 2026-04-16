using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Comment;

public sealed class CommentAggregate : AggregateRoot
{
    private static readonly CommentSpecification Spec = new();
    private readonly List<CommentMention> _mentions = new();

    public CommentId CommentId { get; private set; }
    public string TargetRef { get; private set; } = string.Empty;
    public string AuthorRef { get; private set; } = string.Empty;
    public CommentBody Body { get; private set; } = default!;
    public CommentStatus Status { get; private set; }
    public CommentId? ReplyToId { get; private set; }
    public Timestamp PostedAt { get; private set; }
    public IReadOnlyList<CommentMention> Mentions => _mentions.AsReadOnly();

    private CommentAggregate() { }

    public static CommentAggregate Post(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        CommentId id, string targetRef, string authorRef, CommentBody body, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(authorRef)) throw CommentErrors.InvalidAuthor();
        if (string.IsNullOrWhiteSpace(targetRef)) throw CommentErrors.InvalidTargetRef();
        var agg = new CommentAggregate();
        agg.RaiseDomainEvent(new CommentPostedEvent(eventId, aggregateId, correlationId, causationId, id, targetRef, authorRef, body.Value, at));
        return agg;
    }

    public void Edit(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, CommentBody body, Timestamp at)
    {
        Spec.EnsureMutable(Status);
        RaiseDomainEvent(new CommentEditedEvent(eventId, aggregateId, correlationId, causationId, CommentId, body.Value, at));
    }

    public void Redact(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reason, Timestamp at)
    {
        if (Status == CommentStatus.Redacted) throw CommentErrors.AlreadyRedacted();
        RaiseDomainEvent(new CommentRedactedEvent(eventId, aggregateId, correlationId, causationId, CommentId, reason ?? string.Empty, at));
    }

    public void Flag(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string reporterRef, string reason, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(reporterRef)) throw CommentErrors.InvalidAuthor();
        RaiseDomainEvent(new CommentFlaggedEvent(eventId, aggregateId, correlationId, causationId, CommentId, reporterRef, reason ?? string.Empty, at));
    }

    public void ReplyTo(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, CommentId replyTo, Timestamp at)
    {
        if (replyTo.Value == Guid.Empty) throw CommentErrors.InvalidTargetRef();
        RaiseDomainEvent(new CommentRepliedEvent(eventId, aggregateId, correlationId, causationId, CommentId, replyTo, at));
    }

    public void AddMention(CommentMention mention)
    {
        Spec.EnsureMutable(Status);
        _mentions.Add(mention);
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CommentPostedEvent e:
                CommentId = e.CommentId;
                TargetRef = e.TargetRef;
                AuthorRef = e.AuthorRef;
                Body = CommentBody.Create(e.Body);
                Status = CommentStatus.Posted;
                PostedAt = e.PostedAt;
                break;
            case CommentEditedEvent e:
                Body = CommentBody.Create(e.Body);
                Status = CommentStatus.Edited;
                break;
            case CommentRedactedEvent: Status = CommentStatus.Redacted; break;
            case CommentFlaggedEvent: Status = CommentStatus.Flagged; break;
            case CommentRepliedEvent e: ReplyToId = e.ReplyToId; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Body is null) return;
        if (string.IsNullOrEmpty(AuthorRef)) throw CommentErrors.AuthorMissing();
        if (string.IsNullOrEmpty(TargetRef)) throw CommentErrors.TargetMissing();
    }
}
