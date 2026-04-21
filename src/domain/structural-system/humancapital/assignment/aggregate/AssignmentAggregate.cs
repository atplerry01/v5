using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;
using Whycespace.Domain.StructuralSystem.Humancapital.Participant;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Assignment;

public sealed class AssignmentAggregate : AggregateRoot
{
    public AssignmentId Id { get; private set; }
    public ParticipantId? Participant { get; private set; }
    public ClusterAuthorityRef? Authority { get; private set; }
    public DateTimeOffset? EffectiveAt { get; private set; }

    public static AssignmentAggregate Create()
    {
        var aggregate = new AssignmentAggregate();
        if (aggregate.Version >= 0)
            throw AssignmentErrors.AlreadyInitialized();

        // POLICY HOOK (to be enforced by runtime)
        return aggregate;
    }

    public static AssignmentAggregate Assign(
        AssignmentId id,
        ParticipantId participant,
        ClusterAuthorityRef authority,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "ParentLookup must not be null.");
        Guard.Against(
            effectiveAt == default || effectiveAt == DateTimeOffset.MinValue || effectiveAt == DateTimeOffset.MaxValue,
            "Assignment EffectiveAt must be a concrete, bounded DateTimeOffset.");

        var parentState = parentLookup!.GetState(authority);
        if (parentState != StructuralParentState.Active)
            throw AssignmentErrors.InactiveParent(parentState);

        var aggregate = new AssignmentAggregate();
        if (aggregate.Version >= 0)
            throw AssignmentErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new AssignmentAssignedEvent(id, participant, authority, effectiveAt));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case AssignmentAssignedEvent e:
                Id = e.AssignmentId;
                Participant = e.Participant;
                Authority = e.Authority;
                EffectiveAt = e.EffectiveAt;
                break;
        }
    }
}
