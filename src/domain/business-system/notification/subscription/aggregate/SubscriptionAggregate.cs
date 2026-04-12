namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed class SubscriptionAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SubscriptionId Id { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public SubscriptionTarget Target { get; private set; }
    public int Version { get; private set; }

    private SubscriptionAggregate() { }

    public static SubscriptionAggregate Create(SubscriptionId id, SubscriptionTarget target)
    {
        var aggregate = new SubscriptionAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SubscriptionOptedInEvent(id, target);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void OptOut()
    {
        ValidateBeforeChange();

        var specification = new CanOptOutSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubscriptionErrors.InvalidStateTransition(Status, nameof(OptOut));

        var @event = new SubscriptionOptedOutEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Resubscribe()
    {
        ValidateBeforeChange();

        var specification = new CanResubscribeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SubscriptionErrors.InvalidStateTransition(Status, nameof(Resubscribe));

        var @event = new SubscriptionResubscribedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SubscriptionOptedInEvent @event)
    {
        Id = @event.SubscriptionId;
        Target = @event.Target;
        Status = SubscriptionStatus.OptedIn;
        Version++;
    }

    private void Apply(SubscriptionOptedOutEvent @event)
    {
        Status = SubscriptionStatus.OptedOut;
        Version++;
    }

    private void Apply(SubscriptionResubscribedEvent @event)
    {
        Status = SubscriptionStatus.OptedIn;
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
            throw SubscriptionErrors.MissingId();

        if (Target == default)
            throw SubscriptionErrors.InvalidTarget();

        if (!Enum.IsDefined(Status))
            throw SubscriptionErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
