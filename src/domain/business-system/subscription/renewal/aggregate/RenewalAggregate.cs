namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public sealed class RenewalAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public RenewalId Id { get; private set; }
    public RenewalRequest Request { get; private set; }
    public RenewalStatus Status { get; private set; }
    public int Version { get; private set; }

    private RenewalAggregate() { }

    public static RenewalAggregate Initiate(RenewalId id, RenewalRequest request)
    {
        var aggregate = new RenewalAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new RenewalInitiatedEvent(id, request);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new RenewalCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Fail()
    {
        ValidateBeforeChange();

        var specification = new CanFailSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw RenewalErrors.InvalidStateTransition(Status, nameof(Fail));

        var @event = new RenewalFailedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(RenewalInitiatedEvent @event)
    {
        Id = @event.RenewalId;
        Request = @event.Request;
        Status = RenewalStatus.Pending;
        Version++;
    }

    private void Apply(RenewalCompletedEvent @event)
    {
        Status = RenewalStatus.Renewed;
        Version++;
    }

    private void Apply(RenewalFailedEvent @event)
    {
        Status = RenewalStatus.Failed;
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
            throw RenewalErrors.MissingId();

        if (Request == default)
            throw RenewalErrors.MissingRequest();

        if (!Enum.IsDefined(Status))
            throw RenewalErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
        // Currently no additional pre-conditions beyond specification checks.
    }
}
