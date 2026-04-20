namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Eligibility;

public sealed class EligibilityAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EligibilityId Id { get; private set; }
    public EligibilitySubjectRef Subject { get; private set; }
    public EligibilityTargetRef Target { get; private set; }
    public EligibilityScope Scope { get; private set; }
    public EligibilityStatus Status { get; private set; }
    public IneligibilityReason? Reason { get; private set; }
    public int Version { get; private set; }

    private EligibilityAggregate() { }

    public static EligibilityAggregate Create(
        EligibilityId id,
        EligibilitySubjectRef subject,
        EligibilityTargetRef target,
        EligibilityScope scope)
    {
        var aggregate = new EligibilityAggregate();

        var @event = new EligibilityCreatedEvent(id, subject, target, scope);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void MarkEligible(DateTimeOffset evaluatedAt)
    {
        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.AlreadyEvaluated(Id, Status);

        var @event = new EligibilityEvaluatedEligibleEvent(Id, evaluatedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkIneligible(IneligibilityReason reason, DateTimeOffset evaluatedAt)
    {
        var specification = new CanEvaluateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EligibilityErrors.AlreadyEvaluated(Id, Status);

        var @event = new EligibilityEvaluatedIneligibleEvent(Id, reason, evaluatedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EligibilityCreatedEvent @event)
    {
        Id = @event.EligibilityId;
        Subject = @event.Subject;
        Target = @event.Target;
        Scope = @event.Scope;
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
        Reason = @event.Reason;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw EligibilityErrors.MissingId();

        if (Subject == default)
            throw EligibilityErrors.MissingSubject();

        if (Target == default)
            throw EligibilityErrors.MissingTarget();

        if (!Enum.IsDefined(Status))
            throw EligibilityErrors.InvalidStateTransition(Status, "validate");
    }
}
