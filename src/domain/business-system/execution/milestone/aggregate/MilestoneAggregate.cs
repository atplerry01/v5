namespace Whycespace.Domain.BusinessSystem.Execution.Milestone;

public sealed class MilestoneAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public MilestoneId Id { get; private set; }
    public MilestoneTargetId TargetId { get; private set; }
    public MilestoneStatus Status { get; private set; }
    public int Version { get; private set; }

    private MilestoneAggregate() { }

    public static MilestoneAggregate Create(MilestoneId id, MilestoneTargetId targetId)
    {
        var aggregate = new MilestoneAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new MilestoneCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Reach()
    {
        ValidateBeforeChange();

        var specification = new CanReachSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MilestoneErrors.InvalidStateTransition(Status, nameof(Reach));

        var @event = new MilestoneReachedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Miss()
    {
        ValidateBeforeChange();

        var specification = new CanMissSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw MilestoneErrors.InvalidStateTransition(Status, nameof(Miss));

        var @event = new MilestoneMissedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(MilestoneCreatedEvent @event)
    {
        Id = @event.MilestoneId;
        TargetId = @event.TargetId;
        Status = MilestoneStatus.Defined;
        Version++;
    }

    private void Apply(MilestoneReachedEvent @event)
    {
        Status = MilestoneStatus.Reached;
        Version++;
    }

    private void Apply(MilestoneMissedEvent @event)
    {
        Status = MilestoneStatus.Missed;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);
    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw MilestoneErrors.MissingId();
        if (TargetId == default)
            throw MilestoneErrors.MissingTargetId();
        if (!Enum.IsDefined(Status))
            throw MilestoneErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
