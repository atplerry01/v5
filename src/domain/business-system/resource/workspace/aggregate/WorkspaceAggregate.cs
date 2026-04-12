namespace Whycespace.Domain.BusinessSystem.Resource.Workspace;

public sealed class WorkspaceAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public WorkspaceId Id { get; private set; }
    public WorkspaceStatus Status { get; private set; }
    public WorkspaceScope Scope { get; private set; }
    public WorkspaceLabel Label { get; private set; }
    public int Version { get; private set; }

    private WorkspaceAggregate() { }

    public static WorkspaceAggregate Create(
        WorkspaceId id,
        WorkspaceScope scope,
        WorkspaceLabel label)
    {
        var aggregate = new WorkspaceAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new WorkspaceCreatedEvent(id, scope, label);
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
            throw WorkspaceErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new WorkspaceActivatedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Decommission()
    {
        ValidateBeforeChange();

        var specification = new CanDecommissionSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw WorkspaceErrors.InvalidStateTransition(Status, nameof(Decommission));

        var @event = new WorkspaceDecommissionedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(WorkspaceCreatedEvent @event)
    {
        Id = @event.WorkspaceId;
        Scope = @event.Scope;
        Label = @event.Label;
        Status = WorkspaceStatus.Provisioned;
        Version++;
    }

    private void Apply(WorkspaceActivatedEvent @event)
    {
        Status = WorkspaceStatus.Active;
        Version++;
    }

    private void Apply(WorkspaceDecommissionedEvent @event)
    {
        Status = WorkspaceStatus.Decommissioned;
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
            throw WorkspaceErrors.MissingId();

        if (Scope == default)
            throw WorkspaceErrors.ScopeRequired();

        if (Label == default)
            throw WorkspaceErrors.LabelRequired();

        if (!Enum.IsDefined(Status))
            throw WorkspaceErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
