namespace Whycespace.Domain.BusinessSystem.Notification.Preference;

public sealed class PreferenceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PreferenceId Id { get; private set; }
    public PreferenceStatus Status { get; private set; }
    public PreferenceRule Rule { get; private set; }
    public int Version { get; private set; }

    private PreferenceAggregate() { }

    public static PreferenceAggregate Create(PreferenceId id, PreferenceRule rule)
    {
        var aggregate = new PreferenceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new PreferenceDefinedEvent(id, rule);
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
            throw PreferenceErrors.InvalidStateTransition(Status, nameof(Enforce));

        var @event = new PreferenceEnforcedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PreferenceErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new PreferenceSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reinstate()
    {
        ValidateBeforeChange();

        var specification = new CanReinstateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PreferenceErrors.InvalidStateTransition(Status, nameof(Reinstate));

        var @event = new PreferenceReinstatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PreferenceDefinedEvent @event)
    {
        Id = @event.PreferenceId;
        Rule = @event.Rule;
        Status = PreferenceStatus.Draft;
        Version++;
    }

    private void Apply(PreferenceEnforcedEvent @event)
    {
        Status = PreferenceStatus.Enforced;
        Version++;
    }

    private void Apply(PreferenceSuspendedEvent @event)
    {
        Status = PreferenceStatus.Suspended;
        Version++;
    }

    private void Apply(PreferenceReinstatedEvent @event)
    {
        Status = PreferenceStatus.Enforced;
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
            throw PreferenceErrors.MissingId();

        if (Rule == default)
            throw PreferenceErrors.InvalidRule();

        if (!Enum.IsDefined(Status))
            throw PreferenceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
