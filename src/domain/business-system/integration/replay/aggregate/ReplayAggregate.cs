namespace Whycespace.Domain.BusinessSystem.Integration.Replay;

public sealed class ReplayAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ReplayId Id { get; private set; }
    public ReplayPolicyId PolicyId { get; private set; }
    public ReplayStatus Status { get; private set; }
    public int Version { get; private set; }

    private ReplayAggregate() { }

    public static ReplayAggregate Create(ReplayId id, ReplayPolicyId policyId)
    {
        var aggregate = new ReplayAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ReplayCreatedEvent(id, policyId);
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
            throw ReplayErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ReplayActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Disable()
    {
        ValidateBeforeChange();

        var specification = new CanDisableSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ReplayErrors.InvalidStateTransition(Status, nameof(Disable));

        var @event = new ReplayDisabledEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ReplayCreatedEvent @event)
    {
        Id = @event.ReplayId;
        PolicyId = @event.PolicyId;
        Status = ReplayStatus.Defined;
        Version++;
    }

    private void Apply(ReplayActivatedEvent @event)
    {
        Status = ReplayStatus.Active;
        Version++;
    }

    private void Apply(ReplayDisabledEvent @event)
    {
        Status = ReplayStatus.Disabled;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw ReplayErrors.MissingId();

        if (PolicyId == default)
            throw ReplayErrors.MissingPolicyId();

        if (!Enum.IsDefined(Status))
            throw ReplayErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
