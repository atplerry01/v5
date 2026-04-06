using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Participant;

public sealed class ParticipantAggregate : AggregateRoot
{
    public new ParticipantId Id { get; private set; } = default!;
    public ParticipantIdentityLink IdentityLink { get; private set; } = default!;
    public ParticipantType Type { get; private set; } = default!;
    public EntryLevel Level { get; private set; } = default!;
    public bool IsActive { get; private set; }

    public static ParticipantAggregate Create(Guid participantId, IdentityId identityId, ParticipantType type, EntryLevel level)
    {
        Guard.AgainstDefault(participantId);
        Guard.AgainstNull(identityId);
        Guard.AgainstNull(type);
        Guard.AgainstNull(level);

        var participant = new ParticipantAggregate();
        participant.Apply(new ParticipantRegisteredEvent(participantId, identityId.Value, type.Id, type.Name, level.Id, level.Level));
        return participant;
    }

    public void Activate()
    {
        EnsureInvariant(!IsActive, "ALREADY_IN_STATE", "Participant is already active.");

        Apply(new ParticipantActivatedEvent(Id.Value));
    }

    public void Suspend(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureInvariant(IsActive, "INVALID_STATE", "Participant is already suspended.");

        Apply(new ParticipantSuspendedEvent(Id.Value, reason));
    }

    public void Remove()
    {
        Apply(new ParticipantRemovedEvent(Id.Value));
    }

    public void Upgrade(EntryLevel newLevel)
    {
        Guard.AgainstNull(newLevel);
        EnsureInvariant(IsActive, "INVALID_STATE", "Cannot upgrade an inactive participant.");

        Apply(new ParticipantUpgradedEvent(Id.Value, newLevel.Level));
    }

    private void Apply(ParticipantRegisteredEvent e)
    {
        Id = new ParticipantId(e.ParticipantId);
        IdentityLink = new ParticipantIdentityLink(new IdentityId(e.IdentityId), e.OccurredAt);
        Type = new ParticipantType(e.ParticipantTypeId, e.ParticipantType);
        Level = new EntryLevel(e.EntryLevelId, e.EntryLevel);
        IsActive = false;
        RaiseDomainEvent(e);
    }

    private void Apply(ParticipantActivatedEvent e)
    {
        IsActive = true;
        RaiseDomainEvent(e);
    }

    private void Apply(ParticipantSuspendedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }

    private void Apply(ParticipantRemovedEvent e)
    {
        IsActive = false;
        RaiseDomainEvent(e);
    }

    private void Apply(ParticipantUpgradedEvent e)
    {
        Level = new EntryLevel(Level.Id, e.NewLevel);
        RaiseDomainEvent(e);
    }
}
