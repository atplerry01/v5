using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Entitlement.EligibilityAndGrant.Assignment;

public sealed class AssignmentAggregate : AggregateRoot
{
    public AssignmentId Id { get; private set; }
    public GrantRef Grant { get; private set; }
    public AssignmentSubjectRef Subject { get; private set; }
    public AssignmentScope Scope { get; private set; }
    public AssignmentStatus Status { get; private set; }

    public static AssignmentAggregate Create(
        AssignmentId id,
        GrantRef grant,
        AssignmentSubjectRef subject,
        AssignmentScope scope)
    {
        var aggregate = new AssignmentAggregate();
        if (aggregate.Version >= 0)
            throw AssignmentErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AssignmentCreatedEvent(id, grant, subject, scope));
        return aggregate;
    }

    public void Activate(DateTimeOffset activatedAt)
    {
        var specification = new CanActivateSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssignmentErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new AssignmentActivatedEvent(Id, activatedAt));
    }

    public void Revoke(DateTimeOffset revokedAt)
    {
        var specification = new CanRevokeSpecification();
        if (!specification.IsSatisfiedBy(Status))
            throw AssignmentErrors.AlreadyRevoked(Id);

        RaiseDomainEvent(new AssignmentRevokedEvent(Id, revokedAt));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssignmentCreatedEvent e:
                Id = e.AssignmentId;
                Grant = e.Grant;
                Subject = e.Subject;
                Scope = e.Scope;
                Status = AssignmentStatus.Pending;
                break;
            case AssignmentActivatedEvent:
                Status = AssignmentStatus.Active;
                break;
            case AssignmentRevokedEvent:
                Status = AssignmentStatus.Revoked;
                break;
        }
    }

    protected override void EnsureInvariants()
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
