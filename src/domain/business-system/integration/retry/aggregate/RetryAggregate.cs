namespace Whycespace.Domain.BusinessSystem.Integration.Retry;

public sealed class RetryAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RetryId Id { get; private set; }
    public RetryPolicyId PolicyId { get; private set; }
    public RetryStatus Status { get; private set; }
    public int Version { get; private set; }

    private RetryAggregate() { }

    public static RetryAggregate Create(RetryId id, RetryPolicyId policyId)
    {
        var aggregate = new RetryAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RetryCreatedEvent(id, policyId);
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
            throw RetryErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new RetryActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RetryErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new RetryDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RetryCreatedEvent @event)
    {
        Id = @event.RetryId;
        PolicyId = @event.PolicyId;
        Status = RetryStatus.Defined;
        Version++;
    }

    private void Apply(RetryActivatedEvent @event)
    {
        Status = RetryStatus.Active;
        Version++;
    }

    private void Apply(RetryDisabledEvent @event)
    {
        Status = RetryStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw RetryErrors.MissingId();

        if (PolicyId == default)
            throw RetryErrors.MissingPolicyId();

        if (!Enum.IsDefined(Status))
            throw RetryErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
