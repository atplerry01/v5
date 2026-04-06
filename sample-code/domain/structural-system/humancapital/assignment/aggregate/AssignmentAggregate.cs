using Whycespace.Domain.SharedKernel;
using Whycespace.Domain.SharedKernel.Primitive.Identity;
using Whycespace.Shared.Primitives.Id;

namespace Whycespace.Domain.StructuralSystem.HumanCapital.Assignment;

public sealed class AssignmentAggregate : AggregateRoot
{
    public AssignmentId AssignmentId { get; private set; } = default!;
    public IdentityId AssigneeIdentityId { get; private set; } = default!;
    public AssignmentScope Scope { get; private set; } = default!;
    public AssignmentStatus Status { get; private set; } = AssignmentStatus.Created;
    public TimeWindow TimeWindow { get; private set; } = default!;

    public static AssignmentAggregate Create(Guid assignmentId, IdentityId assigneeIdentityId, string scopeType, DateTimeOffset start, DateTimeOffset end)
    {
        Guard.AgainstDefault(assignmentId);
        Guard.AgainstNull(assigneeIdentityId);
        Guard.AgainstEmpty(scopeType);
        Guard.AgainstInvalid(end, e => e > start, "End must be after start.");

        var assignment = new AssignmentAggregate();
        assignment.Apply(new AssignmentCreatedEvent(assignmentId, assigneeIdentityId.Value, DeterministicIdHelper.FromSeed($"AssignmentScope:{assignmentId}:{scopeType}"), scopeType, start, end));
        return assignment;
    }

    public void Start()
    {
        EnsureInvariant(
            Status == AssignmentStatus.Created,
            "INVALID_STATE_TRANSITION",
            $"Cannot start assignment in '{Status.StatusName}' status.");

        Apply(new AssignmentStartedEvent(Id));
    }

    public void Complete()
    {
        EnsureInvariant(
            Status == AssignmentStatus.Started,
            "INVALID_STATE_TRANSITION",
            $"Cannot complete assignment in '{Status.StatusName}' status.");

        Apply(new AssignmentCompletedEvent(Id));
    }

    public void Fail(string reason)
    {
        Guard.AgainstEmpty(reason);
        EnsureNotTerminal(Status, s => s.IsTerminal, "fail");

        Apply(new AssignmentFailedEvent(Id, reason));
    }

    public void Reassign(IdentityId newAssigneeIdentityId)
    {
        Guard.AgainstNull(newAssigneeIdentityId);
        EnsureNotTerminal(Status, s => s.IsTerminal, "reassign");

        EnsureInvariant(
            newAssigneeIdentityId.Value != AssigneeIdentityId.Value,
            "CONSISTENCY",
            "Cannot reassign to the same assignee.");

        Apply(new AssignmentReassignedEvent(Id, newAssigneeIdentityId.Value));
    }

    private void Apply(AssignmentCreatedEvent e)
    {
        Id = e.AssignmentId;
        AssignmentId = new AssignmentId(e.AssignmentId);
        AssigneeIdentityId = new IdentityId(e.AssigneeIdentityId);
        Scope = new AssignmentScope(e.ScopeId, e.ScopeType);
        Status = AssignmentStatus.Created;
        TimeWindow = new TimeWindow(e.Start, e.End);
        RaiseDomainEvent(e);
    }

    private void Apply(AssignmentStartedEvent e)
    {
        Status = AssignmentStatus.Started;
        RaiseDomainEvent(e);
    }

    private void Apply(AssignmentCompletedEvent e)
    {
        Status = AssignmentStatus.Completed;
        RaiseDomainEvent(e);
    }

    private void Apply(AssignmentFailedEvent e)
    {
        Status = AssignmentStatus.Failed;
        RaiseDomainEvent(e);
    }

    private void Apply(AssignmentReassignedEvent e)
    {
        AssigneeIdentityId = new IdentityId(e.NewAssigneeIdentityId);
        RaiseDomainEvent(e);
    }
}
