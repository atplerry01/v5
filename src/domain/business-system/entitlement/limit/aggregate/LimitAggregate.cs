namespace Whycespace.Domain.BusinessSystem.Entitlement.Limit;

public sealed class LimitAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LimitId Id { get; private set; }
    public LimitSubjectId SubjectId { get; private set; }
    public LimitStatus Status { get; private set; }
    public int ThresholdValue { get; private set; }
    public int Version { get; private set; }

    private LimitAggregate() { }

    public static LimitAggregate Create(LimitId id, LimitSubjectId subjectId, int thresholdValue)
    {
        if (thresholdValue <= 0)
            throw new ArgumentException("Threshold value must be greater than zero.", nameof(thresholdValue));

        var aggregate = new LimitAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new LimitCreatedEvent(id, subjectId, thresholdValue);
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
            throw LimitErrors.InvalidStateTransition(Status, nameof(Enforce));

        var @event = new LimitEnforcedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Breach(int observedValue)
    {
        ValidateBeforeChange();

        if (observedValue <= ThresholdValue)
            throw new ArgumentException("Observed value must exceed threshold to constitute a breach.", nameof(observedValue));

        var specification = new CanBreachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LimitErrors.InvalidStateTransition(Status, nameof(Breach));

        var @event = new LimitBreachedEvent(Id, observedValue);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LimitCreatedEvent @event)
    {
        Id = @event.LimitId;
        SubjectId = @event.SubjectId;
        ThresholdValue = @event.ThresholdValue;
        Status = LimitStatus.Defined;
        Version++;
    }

    private void Apply(LimitEnforcedEvent @event)
    {
        Status = LimitStatus.Enforced;
        Version++;
    }

    private void Apply(LimitBreachedEvent @event)
    {
        Status = LimitStatus.Breached;
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
            throw LimitErrors.MissingId();

        if (SubjectId == default)
            throw LimitErrors.MissingSubjectId();

        if (ThresholdValue <= 0)
            throw LimitErrors.InvalidThreshold();

        if (!Enum.IsDefined(Status))
            throw LimitErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
