namespace Whycespace.Domain.BusinessSystem.Execution.Setup;

public sealed class SetupAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public SetupId Id { get; private set; }
    public SetupTargetId TargetId { get; private set; }
    public SetupStatus Status { get; private set; }
    public int Version { get; private set; }

    private SetupAggregate() { }

    public static SetupAggregate Create(SetupId id, SetupTargetId targetId)
    {
        var aggregate = new SetupAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new SetupCreatedEvent(id, targetId);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Configure()
    {
        ValidateBeforeChange();

        var specification = new CanConfigureSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SetupErrors.InvalidStateTransition(Status, nameof(Configure));

        var @event = new SetupConfiguredEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void MarkReady()
    {
        ValidateBeforeChange();

        var specification = new CanMarkReadySpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw SetupErrors.InvalidStateTransition(Status, nameof(MarkReady));

        var @event = new SetupReadyEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(SetupCreatedEvent @event)
    {
        Id = @event.SetupId;
        TargetId = @event.TargetId;
        Status = SetupStatus.Pending;
        Version++;
    }

    private void Apply(SetupConfiguredEvent @event)
    {
        Status = SetupStatus.Configured;
        Version++;
    }

    private void Apply(SetupReadyEvent @event)
    {
        Status = SetupStatus.Ready;
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
            throw SetupErrors.MissingId();

        if (TargetId == default)
            throw SetupErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw SetupErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
