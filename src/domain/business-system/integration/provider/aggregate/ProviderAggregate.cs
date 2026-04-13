namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class ProviderAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ProviderId Id { get; private set; }
    public ProviderProfile Profile { get; private set; } = null!;
    public ProviderStatus Status { get; private set; }
    public int Version { get; private set; }

    private ProviderAggregate() { }

    public static ProviderAggregate Create(ProviderId id, ProviderProfile profile)
    {
        if (profile is null)
            throw new ArgumentNullException(nameof(profile));

        var aggregate = new ProviderAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ProviderCreatedEvent(id, profile.ConfigId, profile.ProviderName);
        aggregate.Apply(@event, profile);
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

    private void Apply(ProviderCreatedEvent @event, ProviderProfile profile)
    {
        Id = @event.ProviderId;
        Profile = profile;
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

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ProviderErrors.MissingId();

        if (Profile is null)
            throw ProviderErrors.MissingProfile();

        if (!Enum.IsDefined(Status))
            throw ProviderErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
