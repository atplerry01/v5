namespace Whycespace.Domain.BusinessSystem.Entitlement.EntitlementGrant;

public sealed class EntitlementGrantAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public EntitlementGrantId Id { get; private set; }
    public EntitlementAssignment Assignment { get; private set; } = null!;
    public EntitlementGrantStatus Status { get; private set; }
    public int Version { get; private set; }

    private EntitlementGrantAggregate() { }

    public static EntitlementGrantAggregate Create(EntitlementGrantId id, EntitlementAssignment assignment)
    {
        if (assignment is null)
            throw new ArgumentNullException(nameof(assignment));

        var aggregate = new EntitlementGrantAggregate();
        aggregate.ValidateBeforeChange();

        var @event = new EntitlementGrantCreatedEvent(id, assignment.SubjectId, assignment.RightId);
        aggregate.Apply(@event, assignment);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Grant()
    {
        ValidateBeforeChange();

        var specification = new CanGrantSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EntitlementGrantErrors.InvalidStateTransition(Status, nameof(Grant));

        var @event = new EntitlementGrantGrantedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke()
    {
        ValidateBeforeChange();

        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw EntitlementGrantErrors.InvalidStateTransition(Status, nameof(Revoke));

        var @event = new EntitlementGrantRevokedEvent(Id);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(EntitlementGrantCreatedEvent @event, EntitlementAssignment assignment)
    {
        Id = @event.EntitlementGrantId;
        Assignment = assignment;
        Status = EntitlementGrantStatus.Pending;
        Version++;
    }

    private void Apply(EntitlementGrantGrantedEvent @event)
    {
        Status = EntitlementGrantStatus.Granted;
        Version++;
    }

    private void Apply(EntitlementGrantRevokedEvent @event)
    {
        Status = EntitlementGrantStatus.Revoked;
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
            throw EntitlementGrantErrors.MissingId();

        if (Assignment is null)
            throw EntitlementGrantErrors.MissingAssignment();

        if (!Enum.IsDefined(Status))
            throw EntitlementGrantErrors.InvalidStateTransition(Status, "validate");
    }

    private void ValidateBeforeChange()
    {
        // Pre-condition gate: reserved for cross-cutting pre-change validation.
    }
}
