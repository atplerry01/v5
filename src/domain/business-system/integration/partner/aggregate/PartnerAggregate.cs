namespace Whycespace.Domain.BusinessSystem.Integration.Partner;

public sealed class PartnerAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PartnerId Id { get; private set; }
    public PartnerProfile Profile { get; private set; }
    public PartnerStatus Status { get; private set; }
    public int Version { get; private set; }

    private PartnerAggregate() { }

    public static PartnerAggregate Register(PartnerId id, PartnerProfile profile)
    {
        var aggregate = new PartnerAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new PartnerRegisteredEvent(id, profile);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate()
    {
        ValidateBeforeChange();

        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PartnerErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PartnerActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PartnerErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new PartnerSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deregister()
    {
        ValidateBeforeChange();

        var specification = new CanDeregisterSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PartnerErrors.InvalidStateTransition(Status, nameof(Deregister));

        var @event = new PartnerDeregisteredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PartnerRegisteredEvent @event)
    {
        Id = @event.PartnerId;
        Profile = @event.Profile;
        Status = PartnerStatus.Registered;
        Version++;
    }

    private void Apply(PartnerActivatedEvent @event)
    {
        Status = PartnerStatus.Active;
        Version++;
    }

    private void Apply(PartnerSuspendedEvent @event)
    {
        Status = PartnerStatus.Suspended;
        Version++;
    }

    private void Apply(PartnerDeregisteredEvent @event)
    {
        Status = PartnerStatus.Deregistered;
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
            throw PartnerErrors.MissingId();

        if (Profile == default)
            throw PartnerErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw PartnerErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
