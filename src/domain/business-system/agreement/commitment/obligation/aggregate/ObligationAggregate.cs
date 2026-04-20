namespace Whycespace.Domain.BusinessSystem.Agreement.Commitment.Obligation;

public sealed class ObligationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ObligationId Id { get; private set; }
    public ObligationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ObligationAggregate() { }

    public static ObligationAggregate Create(ObligationId id)
    {
        var aggregate = new ObligationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ObligationCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Fulfill()
    {
        ValidateBeforeChange();

        var specification = new CanFulfillSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ObligationErrors.InvalidStateTransition(Status, nameof(Fulfill));

        var @event = new ObligationFulfilledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Breach()
    {
        ValidateBeforeChange();

        var specification = new CanBreachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ObligationErrors.InvalidStateTransition(Status, nameof(Breach));

        var @event = new ObligationBreachedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ObligationCreatedEvent @event)
    {
        Id = @event.ObligationId;
        Status = ObligationStatus.Pending;
        Version++;
    }

    private void Apply(ObligationFulfilledEvent @event)
    {
        Status = ObligationStatus.Fulfilled;
        Version++;
    }

    private void Apply(ObligationBreachedEvent @event)
    {
        Status = ObligationStatus.Breached;
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
            throw ObligationErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw ObligationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
