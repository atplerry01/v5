namespace Whycespace.Domain.BusinessSystem.Portfolio.Performance;

public sealed class PerformanceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public PerformanceId Id { get; private set; }
    public PerformanceName Name { get; private set; }
    public PerformanceStatus Status { get; private set; }
    public int Version { get; private set; }

    private PerformanceAggregate() { }

    public static PerformanceAggregate Create(
        PerformanceId id,
        PerformanceName name)
    {
        var aggregate = new PerformanceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new PerformanceCreatedEvent(id, name);
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
            throw PerformanceErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new PerformanceActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PerformanceErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new PerformanceSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resume()
    {
        ValidateBeforeChange();

        var specification = new CanResumeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PerformanceErrors.InvalidStateTransition(Status, nameof(Resume));

        var @event = new PerformanceResumedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close()
    {
        ValidateBeforeChange();

        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw PerformanceErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new PerformanceClosedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(PerformanceCreatedEvent @event)
    {
        Id = @event.PerformanceId;
        Name = @event.PerformanceName;
        Status = PerformanceStatus.Draft;
        Version++;
    }

    private void Apply(PerformanceActivatedEvent @event)
    {
        Status = PerformanceStatus.Active;
        Version++;
    }

    private void Apply(PerformanceSuspendedEvent @event)
    {
        Status = PerformanceStatus.Suspended;
        Version++;
    }

    private void Apply(PerformanceResumedEvent @event)
    {
        Status = PerformanceStatus.Active;
        Version++;
    }

    private void Apply(PerformanceClosedEvent @event)
    {
        Status = PerformanceStatus.Closed;
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
            throw PerformanceErrors.MissingId();

        if (Name == default)
            throw PerformanceErrors.NameRequired();

        if (!Enum.IsDefined(Status))
            throw PerformanceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
