namespace Whycespace.Domain.BusinessSystem.Entitlement.Eligibility;

public sealed class EligibilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EligibilityId Id { get; private set; }
    public SubjectId SubjectId { get; private set; }
    public EligibilityStatus Status { get; private set; }
    public string CriteriaDescription { get; private set; } = null!;
    public string IneligibilityReason { get; private set; } = null!;
    public int Version { get; private set; }

    private EligibilityAggregate() { }

    public static EligibilityAggregate Create(EligibilityId id, SubjectId subjectId, string criteriaDescription)
    {
        if (string.IsNullOrWhiteSpace(criteriaDescription))
            throw new ArgumentException("Criteria description must not be empty.", nameof(criteriaDescription));

        var aggregate = new EligibilityAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EligibilityCreatedEvent(id, subjectId, criteriaDescription);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void EvaluateEligible()
    {
        ValidateBeforeChange();

        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.InvalidStateTransition(Status, nameof(EvaluateEligible));

        var @event = new EligibilityEvaluatedEligibleEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void EvaluateIneligible(string reason)
    {
        ValidateBeforeChange();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Ineligibility reason must not be empty.", nameof(reason));

        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.InvalidStateTransition(Status, nameof(EvaluateIneligible));

        var @event = new EligibilityEvaluatedIneligibleEvent(Id, reason);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EligibilityCreatedEvent @event)
    {
        Id = @event.EligibilityId;
        SubjectId = @event.SubjectId;
        CriteriaDescription = @event.CriteriaDescription;
        Status = EligibilityStatus.Pending;
        Version++;
    }

    private void Apply(EligibilityEvaluatedEligibleEvent @event)
    {
        Status = EligibilityStatus.Eligible;
        Version++;
    }

    private void Apply(EligibilityEvaluatedIneligibleEvent @event)
    {
        Status = EligibilityStatus.Ineligible;
        IneligibilityReason = @event.Reason;
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
            throw EligibilityErrors.MissingId();

        if (SubjectId == default)
            throw EligibilityErrors.MissingSubjectId();

        if (string.IsNullOrWhiteSpace(CriteriaDescription))
            throw EligibilityErrors.MissingCriteria();

        if (!Enum.IsDefined(Status))
            throw EligibilityErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
