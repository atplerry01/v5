namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed class ProviderAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderId Id { get; private set; }
    public ProviderProfile Profile { get; private set; }
    public ProviderStatus Status { get; private set; }
    public int Version { get; private set; }

    private ProviderAggregate() { }

    public static ProviderAggregate Register(ProviderId id, ProviderProfile profile)
    {
        var aggregate = new ProviderAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ProviderRegisteredEvent(id, profile);
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
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ProviderActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ProviderErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ProviderSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ProviderRegisteredEvent @event)
    {
        Id = @event.ProviderId;
        Profile = @event.Profile;
        Status = ProviderStatus.Registered;
        Version++;
    }

    private void Apply(ProviderActivatedEvent @event)
    {
        Status = ProviderStatus.Active;
        Version++;
    }

    private void Apply(ProviderSuspendedEvent @event)
    {
        Status = ProviderStatus.Suspended;
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
            throw ProviderErrors.MissingId();

        if (Profile == default)
            throw ProviderErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw ProviderErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
