using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Call;

public sealed class CallAggregate : AggregateRoot
{
    private static readonly CallSpecification Spec = new();
    private readonly Dictionary<string, CallParticipant> _participants = new(StringComparer.Ordinal);

    public CallId CallId { get; private set; }
    public string InitiatorRef { get; private set; } = string.Empty;
    public CallMedium Medium { get; private set; }
    public CallStatus Status { get; private set; }
    public Timestamp InitiatedAt { get; private set; }
    public Timestamp? EndedAt { get; private set; }
    public IReadOnlyCollection<CallParticipant> Participants => _participants.Values;

    private CallAggregate() { }

    public static CallAggregate Initiate(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        CallId id, string initiatorRef, CallMedium medium, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(initiatorRef)) throw CallErrors.InvalidActor();
        var agg = new CallAggregate();
        agg.RaiseDomainEvent(new CallInitiatedEvent(eventId, aggregateId, correlationId, causationId, id, initiatorRef, medium, at));
        return agg;
    }

    public void Answer(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string actorRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(actorRef)) throw CallErrors.InvalidActor();
        if (Status != CallStatus.Initiated && Status != CallStatus.Ringing)
            throw CallErrors.NotAnswerable();
        RaiseDomainEvent(new CallAnsweredEvent(eventId, aggregateId, correlationId, causationId, CallId, actorRef, at));
    }

    public void Reject(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string actorRef, Timestamp at)
    {
        if (string.IsNullOrWhiteSpace(actorRef)) throw CallErrors.InvalidActor();
        Spec.EnsureTransition(Status, CallStatus.Rejected);
        RaiseDomainEvent(new CallRejectedEvent(eventId, aggregateId, correlationId, causationId, CallId, actorRef, at));
    }

    public void End(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == CallStatus.Ended) throw CallErrors.AlreadyEnded();
        Spec.EnsureTransition(Status, CallStatus.Ended);
        RaiseDomainEvent(new CallEndedEvent(eventId, aggregateId, correlationId, causationId, CallId, at));
    }

    public void JoinParticipant(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string actorRef, Timestamp at)
    {
        if (Status != CallStatus.Answered) throw CallErrors.NotAnswerable();
        if (string.IsNullOrWhiteSpace(actorRef)) throw CallErrors.InvalidActor();
        if (_participants.TryGetValue(actorRef, out var existing) && existing.IsActive) return;
        RaiseDomainEvent(new CallParticipantJoinedEvent(eventId, aggregateId, correlationId, causationId, CallId, actorRef, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case CallInitiatedEvent e:
                CallId = e.CallId;
                InitiatorRef = e.InitiatorRef;
                Medium = e.Medium;
                Status = CallStatus.Initiated;
                InitiatedAt = e.InitiatedAt;
                break;
            case CallAnsweredEvent e:
                Status = CallStatus.Answered;
                _participants[e.ActorRef] = CallParticipant.Join(e.ActorRef, e.AnsweredAt);
                break;
            case CallRejectedEvent: Status = CallStatus.Rejected; break;
            case CallEndedEvent e:
                Status = CallStatus.Ended;
                EndedAt = e.EndedAt;
                break;
            case CallParticipantJoinedEvent e:
                _participants[e.ActorRef] = CallParticipant.Join(e.ActorRef, e.JoinedAt);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Status == CallStatus.Initiated && string.IsNullOrEmpty(InitiatorRef) && DomainEvents.Count > 0)
            throw CallErrors.InitiatorMissing();
    }
}
