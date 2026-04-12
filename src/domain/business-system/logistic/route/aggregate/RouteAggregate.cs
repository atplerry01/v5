namespace Whycespace.Domain.BusinessSystem.Logistic.Route;

public sealed class RouteAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RouteId Id { get; private set; }
    public RoutePath Path { get; private set; } = null!;
    public RouteStatus Status { get; private set; }
    public int Version { get; private set; }

    private RouteAggregate() { }

    public static RouteAggregate Create(
        RouteId id,
        RoutePath path)
    {
        var aggregate = new RouteAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RouteCreatedEvent(id, path);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Define()
    {
        ValidateBeforeChange();

        var specification = new CanDefineSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RouteErrors.InvalidStateTransition(Status, nameof(Define));

        var @event = new RouteDefinedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Lock()
    {
        ValidateBeforeChange();

        var specification = new CanLockSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RouteErrors.InvalidStateTransition(Status, nameof(Lock));

        var @event = new RouteLockedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RouteCreatedEvent @event)
    {
        Id = @event.RouteId;
        Path = @event.Path;
        Status = RouteStatus.Draft;
        Version++;
    }

    private void Apply(RouteDefinedEvent @event)
    {
        Status = RouteStatus.Defined;
        Version++;
    }

    private void Apply(RouteLockedEvent @event)
    {
        Status = RouteStatus.Locked;
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
            throw RouteErrors.MissingId();

        if (Path is null || Path.Waypoints.Count < 2)
            throw RouteErrors.PathRequired();

        if (!Enum.IsDefined(Status))
            throw RouteErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
