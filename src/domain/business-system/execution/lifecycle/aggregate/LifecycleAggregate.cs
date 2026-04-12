namespace Whycespace.Domain.BusinessSystem.Execution.Lifecycle;

public sealed class LifecycleAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public LifecycleId Id { get; private set; }
    public LifecycleSubjectId SubjectId { get; private set; }
    public LifecycleStatus Status { get; private set; }
    public int Version { get; private set; }

    private LifecycleAggregate() { }

    public static LifecycleAggregate Create(LifecycleId id, LifecycleSubjectId subjectId)
    {
        var aggregate = new LifecycleAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new LifecycleCreatedEvent(id, subjectId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Start()
    {
        ValidateBeforeChange();

        var specification = new CanStartSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Start));

        var @event = new LifecycleStartedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new LifecycleCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Terminate()
    {
        ValidateBeforeChange();

        var specification = new CanTerminateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, nameof(Terminate));

        var @event = new LifecycleTerminatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(LifecycleCreatedEvent @event)
    {
        Id = @event.LifecycleId;
        SubjectId = @event.SubjectId;
        Status = LifecycleStatus.Initialized;
        Version++;
    }

    private void Apply(LifecycleStartedEvent @event)
    {
        Status = LifecycleStatus.Running;
        Version++;
    }

    private void Apply(LifecycleCompletedEvent @event)
    {
        Status = LifecycleStatus.Completed;
        Version++;
    }

    private void Apply(LifecycleTerminatedEvent @event)
    {
        Status = LifecycleStatus.Terminated;
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
            throw LifecycleErrors.MissingId();

        if (SubjectId == default)
            throw LifecycleErrors.MissingSubjectId();

        if (!Enum.IsDefined(Status))
            throw LifecycleErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
