using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Customer.SegmentationAndLifecycle.Lifecycle;

public sealed class LifecycleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LifecycleId Id { get; private set; }
    public CustomerRef Customer { get; private set; }
    public LifecycleStage Stage { get; private set; }
    public LifecycleStatus Status { get; private set; }
    public int Version { get; private set; }

    private LifecycleAggregate() { }

    public static LifecycleAggregate Start(
        LifecycleId id,
        CustomerRef customer,
        LifecycleStage initialStage,
        DateTimeOffset startedAt)
    {
        var aggregate = new LifecycleAggregate();

        var @event = new LifecycleStartedEvent(id, customer, initialStage, startedAt);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void ChangeStage(LifecycleStage to, DateTimeOffset changedAt)
    {
        var specification = new CanChangeStageSpecification();
        if (!specification.IsSatisfiedBy(Status, Stage, to))
        {
            if (Status != LifecycleStatus.Tracking)
                throw LifecycleErrors.ClosedImmutable(Id);
            throw LifecycleErrors.InvalidStageTransition(Stage, to);
        }

        var @event = new LifecycleStageChangedEvent(Id, Stage, to, changedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Close(DateTimeOffset closedAt)
    {
        var specification = new CanCloseSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.ClosedImmutable(Id);

        var @event = new LifecycleClosedEvent(Id, closedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LifecycleStartedEvent @event)
    {
        Id = @event.LifecycleId;
        Customer = @event.Customer;
        Stage = @event.InitialStage;
        Status = LifecycleStatus.Tracking;
        Version++;
    }

    private void Apply(LifecycleStageChangedEvent @event)
    {
        Stage = @event.To;
        Version++;
    }

    private void Apply(LifecycleClosedEvent @event)
    {
        Status = LifecycleStatus.Closed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw LifecycleErrors.MissingId();

        if (Customer == default)
            throw LifecycleErrors.MissingCustomerRef();

        if (!Enum.IsDefined(Stage))
            throw LifecycleErrors.InvalidStageTransition(Stage, Stage);

        if (!Enum.IsDefined(Status))
            throw LifecycleErrors.ClosedImmutable(Id);
    }
}
