namespace Whycespace.Domain.BusinessSystem.Execution.Cost;

public sealed class CostAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<CostComponent> _components = new();

    public CostId Id { get; private set; }
    public CostContextId ContextId { get; private set; }
    public CostStatus Status { get; private set; }
    public IReadOnlyList<CostComponent> Components => _components.AsReadOnly();
    public int Version { get; private set; }

    private CostAggregate() { }

    public static CostAggregate Create(CostId id, CostContextId contextId)
    {
        var aggregate = new CostAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new CostCreatedEvent(id, contextId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddComponent(CostComponent component)
    {
        if (component is null)
            throw new ArgumentNullException(nameof(component));

        _components.Add(component);
    }

    public void Calculate()
    {
        ValidateBeforeChange();

        var specification = new CanCalculateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CostErrors.InvalidStateTransition(Status, nameof(Calculate));

        if (_components.Count == 0)
            throw CostErrors.ComponentRequired();

        var @event = new CostCalculatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Finalize()
    {
        ValidateBeforeChange();

        var specification = new CanFinalizeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw CostErrors.InvalidStateTransition(Status, nameof(Finalize));

        var @event = new CostFinalizedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(CostCreatedEvent @event)
    {
        Id = @event.CostId;
        ContextId = @event.ContextId;
        Status = CostStatus.Pending;
        Version++;
    }

    private void Apply(CostCalculatedEvent @event)
    {
        Status = CostStatus.Calculated;
        Version++;
    }

    private void Apply(CostFinalizedEvent @event)
    {
        Status = CostStatus.Finalized;
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
            throw CostErrors.MissingId();

        if (ContextId == default)
            throw CostErrors.MissingContextId();

        if (!Enum.IsDefined(Status))
            throw CostErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
