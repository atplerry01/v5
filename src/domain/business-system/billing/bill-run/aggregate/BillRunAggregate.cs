namespace Whycespace.Domain.BusinessSystem.Billing.BillRun;

public sealed class BillRunAggregate
{
    private readonly List<object> _uncommittedEvents = new();
    private readonly List<BillRunItem> _items = new();

    public BillRunId Id { get; private set; }
    public BillRunStatus Status { get; private set; }
    public IReadOnlyList<BillRunItem> Items => _items.AsReadOnly();
    public int Version { get; private set; }

    private BillRunAggregate() { }

    public static BillRunAggregate Create(BillRunId id)
    {
        var aggregate = new BillRunAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new BillRunCreatedEvent(id);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void AddItem(BillRunItem item)
    {
        if (item is null)
            throw new ArgumentNullException(nameof(item));

        _items.Add(item);
    }

    public void Start()
    {
        ValidateBeforeChange();

        var specification = new CanStartSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BillRunErrors.InvalidStateTransition(Status, nameof(Start));

        if (_items.Count == 0)
            throw BillRunErrors.ItemRequired();

        var @event = new BillRunStartedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BillRunErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new BillRunCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Fail()
    {
        ValidateBeforeChange();

        var specification = new CanFailSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw BillRunErrors.InvalidStateTransition(Status, nameof(Fail));

        var @event = new BillRunFailedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(BillRunCreatedEvent @event)
    {
        Id = @event.BillRunId;
        Status = BillRunStatus.Created;
        Version++;
    }

    private void Apply(BillRunStartedEvent @event)
    {
        Status = BillRunStatus.Running;
        Version++;
    }

    private void Apply(BillRunCompletedEvent @event)
    {
        Status = BillRunStatus.Completed;
        Version++;
    }

    private void Apply(BillRunFailedEvent @event)
    {
        Status = BillRunStatus.Failed;
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
            throw BillRunErrors.MissingId();

        if (!Enum.IsDefined(Status))
            throw BillRunErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
