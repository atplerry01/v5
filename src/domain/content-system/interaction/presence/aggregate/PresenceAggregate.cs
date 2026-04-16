using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Presence;

public sealed class PresenceAggregate : AggregateRoot
{
    private static readonly PresenceSpecification Spec = new();

    public PresenceId PresenceId { get; private set; }
    public string ActorRef { get; private set; } = string.Empty;
    public PresenceStatus Status { get; private set; }
    public Timestamp RegisteredAt { get; private set; }
    public Timestamp? LastHeartbeat { get; private set; }

    private PresenceAggregate() { }

    public static PresenceAggregate Register(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        PresenceId id, string actorRef, PresenceStatus initialStatus, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(actorRef)) throw PresenceErrors.InvalidActor();
        var agg = new PresenceAggregate();
        agg.RaiseDomainEvent(new PresenceRegisteredEvent(
            eventId, aggregateId, correlationId, causationId, id, actorRef, initialStatus, at));
        return agg;
    }

    public void ChangeStatus(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, PresenceStatus next, Timestamp at)
    {
        Spec.EnsureCanChangeTo(Status, next);
        if (Status == next) return;
        RaiseDomainEvent(new PresenceStatusChangedEvent(eventId, aggregateId, correlationId, causationId, PresenceId, next, at));
    }

    public void Heartbeat(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == PresenceStatus.Expired) throw PresenceErrors.AlreadyExpired();
        RaiseDomainEvent(new PresenceHeartbeatRecordedEvent(eventId, aggregateId, correlationId, causationId, PresenceId, at));
    }

    public void Expire(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == PresenceStatus.Expired) throw PresenceErrors.AlreadyExpired();
        RaiseDomainEvent(new PresenceExpiredEvent(eventId, aggregateId, correlationId, causationId, PresenceId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case PresenceRegisteredEvent e:
                PresenceId = e.PresenceId;
                ActorRef = e.ActorRef;
                Status = e.InitialStatus;
                RegisteredAt = e.RegisteredAt;
                LastHeartbeat = e.RegisteredAt;
                break;
            case PresenceStatusChangedEvent e: Status = e.Status; break;
            case PresenceHeartbeatRecordedEvent e: LastHeartbeat = e.HeartbeatAt; break;
            case PresenceExpiredEvent: Status = PresenceStatus.Expired; break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (string.IsNullOrEmpty(ActorRef) && Status != PresenceStatus.Offline && _domainState())
            throw PresenceErrors.ActorMissing();
    }

    private bool _domainState() => DomainEvents.Count > 0 || Version >= 0;
}
