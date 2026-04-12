namespace Whycespace.Domain.BusinessSystem.Subscription.SubscriptionAccount;

public sealed class SubscriptionAccountAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SubscriptionAccountId Id { get; private set; }
    public AccountHolder AccountHolder { get; private set; }
    public SubscriptionAccountStatus Status { get; private set; }
    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private SubscriptionAccountAggregate() { }

    public static SubscriptionAccountAggregate Open(SubscriptionAccountId id, AccountHolder holder)
    {
        var aggregate = new SubscriptionAccountAggregate();
        var @event = new SubscriptionAccountOpenedEvent(id, holder);
        aggregate.Apply(@event);
        aggregate.EnsureInvariants();
        aggregate._uncommittedEvents.Add(@event);
        return aggregate;
    }

    public void Activate()
    {
        if (!CanActivateSpecification.IsSatisfiedBy(Status))
            throw SubscriptionAccountErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new SubscriptionAccountActivatedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Suspend()
    {
        if (!CanSuspendSpecification.IsSatisfiedBy(Status))
            throw SubscriptionAccountErrors.InvalidStateTransition(Status, nameof(Suspend));

        var @event = new SubscriptionAccountSuspendedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Reactivate()
    {
        if (!CanReactivateSpecification.IsSatisfiedBy(Status))
            throw SubscriptionAccountErrors.InvalidStateTransition(Status, nameof(Reactivate));

        var @event = new SubscriptionAccountActivatedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    public void Close()
    {
        if (!CanCloseSpecification.IsSatisfiedBy(Status))
            throw SubscriptionAccountErrors.InvalidStateTransition(Status, nameof(Close));

        var @event = new SubscriptionAccountClosedEvent(Id);
        Apply(@event);
        EnsureInvariants();
        _uncommittedEvents.Add(@event);
    }

    private void Apply(SubscriptionAccountOpenedEvent @event)
    {
        Id = @event.SubscriptionAccountId;
        AccountHolder = @event.AccountHolder;
        Status = SubscriptionAccountStatus.Created;
    }

    private void Apply(SubscriptionAccountActivatedEvent _)
    {
        Status = SubscriptionAccountStatus.Active;
    }

    private void Apply(SubscriptionAccountSuspendedEvent _)
    {
        Status = SubscriptionAccountStatus.Suspended;
    }

    private void Apply(SubscriptionAccountClosedEvent _)
    {
        Status = SubscriptionAccountStatus.Closed;
    }

    private void EnsureInvariants()
    {
        if (Id == default)
            throw SubscriptionAccountErrors.MissingId();

        if (AccountHolder == default)
            throw SubscriptionAccountErrors.MissingAccountHolder();

        if (!Enum.IsDefined(Status))
            throw SubscriptionAccountErrors.InvalidStateTransition(Status, "EnsureInvariants");
    }
}
