namespace Whycespace.Domain.BusinessSystem.Execution.Stage;

public sealed class StageAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public StageId Id { get; private set; }
    public StageContextId ContextId { get; private set; }
    public StageStatus Status { get; private set; }
    public int Version { get; private set; }

    private StageAggregate() { }

    public static StageAggregate Create(StageId id, StageContextId contextId)
    {
        var aggregate = new StageAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new StageCreatedEvent(id, contextId);
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
            throw StageErrors.InvalidStateTransition(Status, nameof(Start));

        var @event = new StageStartedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Complete()
    {
        ValidateBeforeChange();

        var specification = new CanCompleteSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw StageErrors.InvalidStateTransition(Status, nameof(Complete));

        var @event = new StageCompletedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(StageCreatedEvent @event)
    {
        Id = @event.StageId;
        ContextId = @event.ContextId;
        Status = StageStatus.Initialized;
        Version++;
    }

    private void Apply(StageStartedEvent @event)
    {
        Status = StageStatus.InProgress;
        Version++;
    }

    private void Apply(StageCompletedEvent @event)
    {
        Status = StageStatus.Completed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw StageErrors.MissingId();
        if (ContextId == default)
            throw StageErrors.MissingContextId();
        if (!Enum.IsDefined(Status))
            throw StageErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
