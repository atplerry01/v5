namespace Whycespace.Domain.BusinessSystem.Integration.Subscription;

public sealed class SubscriptionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SubscriptionId Id { get; private set; }
    public SubscriptionTargetId TargetId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public int Version { get; private set; }

    private SubscriptionAggregate() { }

    public static SubscriptionAggregate Create(SubscriptionId id, SubscriptionTargetId targetId)
    {
        var aggregate = new SubscriptionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SubscriptionCreatedEvent(id, targetId);
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
            throw SubscriptionErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SubscriptionActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubscriptionErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new SubscriptionDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SubscriptionCreatedEvent @event)
    {
        Id = @event.SubscriptionId;
        TargetId = @event.TargetId;
        Status = SubscriptionStatus.Defined;
        Version++;
    }

    private void Apply(SubscriptionActivatedEvent @event)
    {
        Status = SubscriptionStatus.Active;
        Version++;
    }

    private void Apply(SubscriptionDeactivatedEvent @event)
    {
        Status = SubscriptionStatus.Deactivated;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SubscriptionErrors.MissingId();

        if (TargetId == default)
            throw SubscriptionErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw SubscriptionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
