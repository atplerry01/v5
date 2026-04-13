namespace Whycespace.Domain.BusinessSystem.Entitlement.Restriction;

public sealed class RestrictionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RestrictionId Id { get; private set; }
    public RestrictionSubjectId SubjectId { get; private set; }
    public RestrictionStatus Status { get; private set; }
    public string ConditionDescription { get; private set; } = null!;
    public string ViolationReason { get; private set; } = null!;
    public int Version { get; private set; }

    private RestrictionAggregate() { }

    public static RestrictionAggregate Create(RestrictionId id, RestrictionSubjectId subjectId, string conditionDescription)
    {
        if (string.IsNullOrWhiteSpace(conditionDescription))
            throw new ArgumentException("Condition description must not be empty.", nameof(conditionDescription));

        var aggregate = new RestrictionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RestrictionCreatedEvent(id, subjectId, conditionDescription);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Violate(string violationReason)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(violationReason))
            throw new ArgumentException("Violation reason must not be empty.", nameof(violationReason));

        var specification = new CanViolateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RestrictionErrors.InvalidStateTransition(Status, nameof(Violate));

        var @event = new RestrictionViolatedEvent(Id, violationReason);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Lift()
    {
        ValidateBeforeChange();

        var specification = new CanLiftSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RestrictionErrors.InvalidStateTransition(Status, nameof(Lift));

        var @event = new RestrictionLiftedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RestrictionCreatedEvent @event)
    {
        Id = @event.RestrictionId;
        SubjectId = @event.SubjectId;
        ConditionDescription = @event.ConditionDescription;
        Status = RestrictionStatus.Active;
        Version++;
    }

    private void Apply(RestrictionViolatedEvent @event)
    {
        Status = RestrictionStatus.Violated;
        ViolationReason = @event.ViolationReason;
        Version++;
    }

    private void Apply(RestrictionLiftedEvent @event)
    {
        Status = RestrictionStatus.Lifted;
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
            throw RestrictionErrors.MissingId();

        if (SubjectId == default)
            throw RestrictionErrors.MissingSubjectId();

        if (string.IsNullOrWhiteSpace(ConditionDescription))
            throw RestrictionErrors.MissingCondition();

        if (!Enum.IsDefined(Status))
            throw RestrictionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
