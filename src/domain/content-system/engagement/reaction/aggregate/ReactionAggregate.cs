using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Engagement.Reaction;

public sealed class ReactionAggregate : AggregateRoot
{
    private static readonly ReactionSpecification Spec = new();

    public ReactionId ReactionId { get; private set; }
    public string TargetRef { get; private set; } = string.Empty;
    public string ActorRef { get; private set; } = string.Empty;
    public ReactionKind Kind { get; private set; }
    public ReactionStatus Status { get; private set; }
    public Timestamp AddedAt { get; private set; }

    private ReactionAggregate() { }

    public static ReactionAggregate Add(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ReactionId id, string targetRef, string actorRef, ReactionKind kind, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(actorRef)) throw ReactionErrors.InvalidActor();
        if (string.IsNullOrWhiteSpace(targetRef)) throw ReactionErrors.InvalidTargetRef();
        var agg = new ReactionAggregate();
        agg.RaiseDomainEvent(new ReactionAddedEvent(eventId, aggregateId, correlationId, causationId, id, targetRef, actorRef, kind, at));
        return agg;
    }

    public void Change(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, ReactionKind newKind, Timestamp at)
    {
        Spec.EnsureActive(Status);
        if (Kind == newKind) throw ReactionErrors.SameKind();
        RaiseDomainEvent(new ReactionChangedEvent(eventId, aggregateId, correlationId, causationId, ReactionId, newKind, at));
    }

    public void Remove(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        Spec.EnsureActive(Status);
        RaiseDomainEvent(new ReactionRemovedEvent(eventId, aggregateId, correlationId, causationId, ReactionId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ReactionAddedEvent e:
                ReactionId = e.ReactionId;
                TargetRef = e.TargetRef;
                ActorRef = e.ActorRef;
                Kind = e.Kind;
                Status = ReactionStatus.Added;
                AddedAt = e.AddedAt;
                break;
            case ReactionChangedEvent e:
                Kind = e.Kind;
                Status = ReactionStatus.Changed;
                break;
            case ReactionRemovedEvent: Status = ReactionStatus.Removed; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (DomainEvents.Count > 0)
        {
            if (string.IsNullOrEmpty(ActorRef)) throw ReactionErrors.ActorMissing();
            if (string.IsNullOrEmpty(TargetRef)) throw ReactionErrors.TargetMissing();
        }
    }
}
