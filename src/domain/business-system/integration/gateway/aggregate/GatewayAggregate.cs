namespace Whycespace.Domain.BusinessSystem.Integration.Gateway;

public sealed class GatewayAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public GatewayId Id { get; private set; }
    public GatewayRouteId RouteId { get; private set; }
    public GatewayStatus Status { get; private set; }
    public int Version { get; private set; }

    private GatewayAggregate() { }

    public static GatewayAggregate Create(GatewayId id, GatewayRouteId routeId)
    {
        var aggregate = new GatewayAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new GatewayCreatedEvent(id, routeId);
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
            throw GatewayErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new GatewayActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw GatewayErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new GatewayDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(GatewayCreatedEvent @event)
    {
        Id = @event.GatewayId;
        RouteId = @event.RouteId;
        Status = GatewayStatus.Defined;
        Version++;
    }

    private void Apply(GatewayActivatedEvent @event)
    {
        Status = GatewayStatus.Active;
        Version++;
    }

    private void Apply(GatewayDisabledEvent @event)
    {
        Status = GatewayStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw GatewayErrors.MissingId();

        if (RouteId == default)
            throw GatewayErrors.MissingRouteId();

        if (!Enum.IsDefined(Status))
            throw GatewayErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
