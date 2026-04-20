namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed class AssignmentAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public AssignmentId Id { get; private set; }
    public GrantRef Grant { get; private set; }
    public AssignmentSubjectRef Subject { get; private set; }
    public AssignmentScope Scope { get; private set; }
    public AssignmentStatus Status { get; private set; }
    public int Version { get; private set; }

    private AssignmentAggregate() { }

    public static AssignmentAggregate Create(
        AssignmentId id,
        GrantRef grant,
        AssignmentSubjectRef subject,
        AssignmentScope scope)
    {
        var aggregate = new AssignmentAggregate();

        var @event = new AssignmentCreatedEvent(id, grant, subject, scope);
        aggregate.Apply(@event);
        aggregate.AddEvent(@event);
        aggregate.EnsureInvariants();

        return aggregate;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssignmentErrors.InvalidStateTransition(Status, nameof(Activate));

        var @event = new AssignmentActivatedEvent(Id, activatedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssignmentErrors.AlreadyRevoked(Id);

        var @event = new AssignmentRevokedEvent(Id, revokedAt);
        Apply(@event);
        AddEvent(@event);
        EnsureInvariants();
    }

    private void Apply(AssignmentCreatedEvent @event)
    {
        Id = @event.AssignmentId;
        Grant = @event.Grant;
        Subject = @event.Subject;
        Scope = @event.Scope;
        Status = AssignmentStatus.Pending;
        Version++;
    }

    private void Apply(AssignmentActivatedEvent @event)
    {
        Status = AssignmentStatus.Active;
        Version++;
    }

    private void Apply(AssignmentRevokedEvent @event)
    {
        Status = AssignmentStatus.Revoked;
        Version++;
    }

    private void AddEvent(object @event) => _uncommittedEvents.Add(@event);

    public IReadOnlyList<object> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();

    private void EnsureInvariants()
    {
        if (Id == default)
            throw AssignmentErrors.MissingId();

        if (Grant == default)
            throw AssignmentErrors.MissingGrantRef();

        if (Subject == default)
            throw AssignmentErrors.MissingSubject();

        if (!Enum.IsDefined(Status))
            throw AssignmentErrors.InvalidStateTransition(Status, "validate");
    }
}
