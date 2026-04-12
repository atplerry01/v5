namespace Whycespace.Domain.BusinessSystem.Inventory.Valuation;

public sealed class ValuationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ValuationId Id { get; private set; }
    public ValuationMethod Method { get; private set; }
    public ValuationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ValuationAggregate() { }

    public static ValuationAggregate Create(ValuationId id, ValuationMethod method)
    {
        if (!Enum.IsDefined(method))
            throw ValuationErrors.InvalidMethod();

        var aggregate = new ValuationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ValuationCreatedEvent(id, method);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Suspend()
    {
        ValidateBeforeChange();

        var specification = new CanSuspendValuationSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValuationErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new ValuationSuspendedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Reactivate()
    {
        ValidateBeforeChange();

        var specification = new CanReactivateValuationSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValuationErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new ValuationReactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateValuationSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ValuationErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ValuationDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ValuationCreatedEvent @event)
    {
        Id = @event.ValuationId;
        Method = @event.Method;
        Status = ValuationStatus.Active;
        Version++;
    }

    private void Apply(ValuationSuspendedEvent @event)
    {
        Status = ValuationStatus.Suspended;
        Version++;
    }

    private void Apply(ValuationReactivatedEvent @event)
    {
        Status = ValuationStatus.Active;
        Version++;
    }

    private void Apply(ValuationDeactivatedEvent @event)
    {
        Status = ValuationStatus.Deactivated;
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
            throw ValuationErrors.MissingId();

        if (!Enum.IsDefined(Method))
            throw ValuationErrors.InvalidMethod();

        if (!Enum.IsDefined(Status))
            throw ValuationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
