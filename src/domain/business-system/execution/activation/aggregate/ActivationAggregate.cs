namespace Whycespace.Domain.BusinessSystem.Execution.Activation;

public sealed class ActivationAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ActivationId Id { get; private set; }
    public ActivationTargetId TargetId { get; private set; }
    public ActivationStatus Status { get; private set; }
    public int Version { get; private set; }

    private ActivationAggregate() { }

    public static ActivationAggregate Create(ActivationId id, ActivationTargetId targetId)
    {
        var aggregate = new ActivationAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new ActivationCreatedEvent(id, targetId);
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
            throw ActivationErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new ActivationActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Deactivate()
    {
        ValidateBeforeChange();

        var specification = new CanDeactivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw ActivationErrors.InvalidStateTransition(Status, nameof(Deactivate));

        var @event = new ActivationDeactivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(ActivationCreatedEvent @event)
    {
        Id = @event.ActivationId;
        TargetId = @event.TargetId;
        Status = ActivationStatus.Pending;
        Version++;
    }

    private void Apply(ActivationActivatedEvent @event)
    {
        Status = ActivationStatus.Active;
        Version++;
    }

    private void Apply(ActivationDeactivatedEvent @event)
    {
        Status = ActivationStatus.Deactivated;
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
            throw ActivationErrors.MissingId();

        if (TargetId == default)
            throw ActivationErrors.MissingTargetId();

        if (!Enum.IsDefined(Status))
            throw ActivationErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
