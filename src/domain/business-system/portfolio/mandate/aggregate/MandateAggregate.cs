namespace Whycespace.Domain.BusinessSystem.Portfolio.Mandate;

public sealed class MandateAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MandateId Id { get; private set; }
    public MandateName Name { get; private set; }
    public MandateStatus Status { get; private set; }
    public int Version { get; private set; }

    private MandateAggregate() { }

    public static MandateAggregate Create(
        MandateId id,
        MandateName name)
    {
        var aggregate = new MandateAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MandateCreatedEvent(id, name);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Enforce()
    {
        ValidateBeforeChange();

        var specification = new CanEnforceSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MandateErrors.InvalidStateTransition(Status, nameof(Enforce));

        var @event = new MandateEnforcedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke()
    {
        ValidateBeforeChange();

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MandateErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new MandateRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(MandateCreatedEvent @event)
    {
        Id = @event.MandateId;
        Name = @event.MandateName;
        Status = MandateStatus.Draft;
        Version++;
    }

    private void Apply(MandateEnforcedEvent @event)
    {
        Status = MandateStatus.Enforced;
        Version++;
    }

    private void Apply(MandateRevokedEvent @event)
    {
        Status = MandateStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event)
    {
        _uncommittedEvents.Add(@event);
    }

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MandateErrors.MissingId();

        if (Name == default)
            throw MandateErrors.NameRequired();

        if (!Enum.IsDefined(Status))
            throw MandateErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
