using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.ContentSystem.Interaction.Conversation;

public sealed class ConversationAggregate : AggregateRoot
{
    private static readonly ConversationSpecification Spec = new();
    private readonly Dictionary<string, Participant> _participants = new(StringComparer.Ordinal);

    public ConversationId ConversationId { get; private set; }
    public ConversationTopic Topic { get; private set; } = default!;
    public ConversationStatus Status { get; private set; }
    public Timestamp StartedAt { get; private set; }
    public IReadOnlyCollection<Participant> Participants => _participants.Values;

    private ConversationAggregate() { }

    public static ConversationAggregate Start(
        EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId,
        ConversationId id, ConversationTopic topic, string initiatorRef, Timestamp startedAt)
    {
        Spec.EnsureSatisfied(new ConversationStartCandidate(id, topic, initiatorRef));
        var aggregate = new ConversationAggregate();
        aggregate.RaiseDomainEvent(new ConversationStartedEvent(
            eventId, aggregateId, correlationId, causationId, id, topic.Value, initiatorRef, startedAt));
        aggregate.RaiseDomainEvent(new ParticipantJoinedEvent(
            eventId, aggregateId, correlationId, causationId, id, initiatorRef, startedAt));
        return aggregate;
    }

    public void JoinParticipant(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string participantRef, Timestamp joinedAt)
    {
        if (Status == ConversationStatus.Archived) throw ConversationErrors.CannotMutateArchived();
        if (string.IsNullOrWhiteSpace(participantRef)) throw ConversationErrors.InvalidParticipant();
        if (_participants.TryGetValue(participantRef, out var existing) && existing.IsActive)
            throw ConversationErrors.ParticipantAlreadyInConversation();
        RaiseDomainEvent(new ParticipantJoinedEvent(eventId, aggregateId, correlationId, causationId, ConversationId, participantRef, joinedAt));
    }

    public void LeaveParticipant(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, string participantRef, Timestamp leftAt)
    {
        if (Status == ConversationStatus.Archived) throw ConversationErrors.CannotMutateArchived();
        if (!_participants.TryGetValue(participantRef, out var existing) || !existing.IsActive)
            throw ConversationErrors.ParticipantNotInConversation();
        RaiseDomainEvent(new ParticipantLeftEvent(eventId, aggregateId, correlationId, causationId, ConversationId, participantRef, leftAt));
    }

    public void Rename(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, ConversationTopic topic, Timestamp at)
    {
        if (Status == ConversationStatus.Archived) throw ConversationErrors.CannotMutateArchived();
        RaiseDomainEvent(new ConversationRenamedEvent(eventId, aggregateId, correlationId, causationId, ConversationId, topic.Value, at));
    }

    public void Archive(EventId eventId, AggregateId aggregateId, CorrelationId correlationId, CausationId causationId, Timestamp at)
    {
        if (Status == ConversationStatus.Archived) throw ConversationErrors.AlreadyArchived();
        RaiseDomainEvent(new ConversationArchivedEvent(eventId, aggregateId, correlationId, causationId, ConversationId, at));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ConversationStartedEvent e:
                ConversationId = e.ConversationId;
                Topic = ConversationTopic.Create(e.Topic);
                Status = ConversationStatus.Active;
                StartedAt = e.StartedAt;
                break;
            case ParticipantJoinedEvent e:
                _participants[e.ParticipantRef] = Participant.Join(e.ParticipantRef, e.JoinedAt);
                break;
            case ParticipantLeftEvent e:
                if (_participants.TryGetValue(e.ParticipantRef, out var p)) p.Leave(e.LeftAt);
                break;
            case ConversationRenamedEvent e:
                Topic = ConversationTopic.Create(e.Topic);
                break;
            case ConversationArchivedEvent:
                Status = ConversationStatus.Archived;
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (Topic is null) return;
        if (Status == ConversationStatus.Active && !_participants.Values.Any(p => p.IsActive))
            throw ConversationErrors.AtLeastOneParticipant();
    }
}
