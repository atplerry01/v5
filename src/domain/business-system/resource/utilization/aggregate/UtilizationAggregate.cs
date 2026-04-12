namespace Whycespace.Domain.BusinessSystem.Resource.Utilization;

public sealed class UtilizationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public UtilizationId Id { get; private set; }
    public UtilizationStatus Status { get; private set; }
    public ResourceReference ResourceReference { get; private set; }
    public CapacityLimit CapacityLimit { get; private set; }
    public UsageAmount CumulativeUsage { get; private set; }
    public int Version { get; private set; }

    private UtilizationAggregate() { }

    public static UtilizationAggregate Create(
        UtilizationId id,
        ResourceReference resourceReference,
        CapacityLimit capacityLimit)
    {
        var aggregate = new UtilizationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new UtilizationCreatedEvent(id, resourceReference, capacityLimit);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void StartRecording()
    {
        ValidateBeforeChange();

        var specification = new CanStartRecordingSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UtilizationErrors.InvalidStateTransition(Status, nameof(StartRecording));

        var @event = new UtilizationRecordingStartedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void RecordUsage(UsageAmount amount)
    {
        ValidateBeforeChange();

        var specification = new CanRecordUsageSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UtilizationErrors.InvalidStateTransition(Status, nameof(RecordUsage));

        var projectedUsage = CumulativeUsage.Value + amount.Value;
        if (projectedUsage > CapacityLimit.Value)
            throw UtilizationErrors.ExceedsCapacityConstraint(projectedUsage, CapacityLimit.Value);

        var @event = new UtilizationRecordedEvent(Id, amount);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw UtilizationErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new UtilizationCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(UtilizationCreatedEvent @event)
    {
        Id = @event.UtilizationId;
        ResourceReference = @event.ResourceReference;
        CapacityLimit = @event.CapacityLimit;
        CumulativeUsage = new UsageAmount(0);
        Status = UtilizationStatus.Initiated;
        Version++;
    }

    private void Apply(UtilizationRecordingStartedEvent @event)
    {
        Status = UtilizationStatus.Recording;
        Version++;
    }

    private void Apply(UtilizationRecordedEvent @event)
    {
        CumulativeUsage = new UsageAmount(CumulativeUsage.Value + @event.UsageAmount.Value);
        Version++;
    }

    private void Apply(UtilizationCompletedEvent @event)
    {
        Status = UtilizationStatus.Completed;
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
            throw UtilizationErrors.MissingId();

        if (ResourceReference == default)
            throw UtilizationErrors.ResourceReferenceRequired();

        if (CapacityLimit == default)
            throw UtilizationErrors.CapacityLimitRequired();

        if (CumulativeUsage.Value > CapacityLimit.Value)
            throw UtilizationErrors.ExceedsCapacityConstraint(CumulativeUsage.Value, CapacityLimit.Value);

        if (!Enum.IsDefined(Status))
            throw UtilizationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
