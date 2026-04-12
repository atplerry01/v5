namespace Whycespace.Domain.BusinessSystem.Portfolio.Allocation;

public sealed class AllocationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AllocationId Id { get; private set; }
    public AllocationPortfolioReference PortfolioReference { get; private set; }
    public TargetReference TargetReference { get; private set; }
    public AllocationWeight Weight { get; private set; }
    public AllocationStatus Status { get; private set; }
    public int Version { get; private set; }

    private AllocationAggregate() { }

    public static AllocationAggregate Create(
        AllocationId id,
        AllocationPortfolioReference portfolioReference,
        TargetReference targetReference,
        AllocationWeight weight)
    {
        var aggregate = new AllocationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new AllocationCreatedEvent(id, portfolioReference, targetReference, weight);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Apply()
    {
        ValidateBeforeChange();

        var specification = new CanApplySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Apply));

        var @event = new AllocationAppliedEvent(Id);
        ApplyEvent(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revert()
    {
        ValidateBeforeChange();

        var specification = new CanRevertSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AllocationErrors.InvalidStateTransition(Status, nameof(Revert));

        var @event = new AllocationRevertedEvent(Id);
        ApplyEvent(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AllocationCreatedEvent @event)
    {
        Id = @event.AllocationId;
        PortfolioReference = @event.PortfolioReference;
        TargetReference = @event.TargetReference;
        Weight = @event.Weight;
        Status = AllocationStatus.Proposed;
        Version++;
    }

    private void ApplyEvent(AllocationAppliedEvent @event)
    {
        Status = AllocationStatus.Applied;
        Version++;
    }

    private void ApplyEvent(AllocationRevertedEvent @event)
    {
        Status = AllocationStatus.Reverted;
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
            throw AllocationErrors.MissingId();

        if (PortfolioReference == default)
            throw AllocationErrors.PortfolioReferenceRequired();

        if (TargetReference == default)
            throw AllocationErrors.TargetReferenceRequired();

        if (Weight == default)
            throw AllocationErrors.WeightOutOfBounds();

        if (!Enum.IsDefined(Status))
            throw AllocationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
