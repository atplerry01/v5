using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Humancapital.Participant;

public sealed class ParticipantAggregate : AggregateRoot
{
    public ParticipantId Id { get; private set; } = null!;
    public ClusterRef? HomeCluster { get; private set; }
    public DateTimeOffset? PlacedAt { get; private set; }

    public static ParticipantAggregate Register(ParticipantId id)
    {
        var aggregate = new ParticipantAggregate();
        if (aggregate.Version >= 0)
            throw ParticipantErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ParticipantRegisteredEvent(id.Value));
        return aggregate;
    }

    public static ParticipantAggregate Place(
        ParticipantId id,
        ClusterRef homeCluster,
        DateTimeOffset effectiveAt,
        IStructuralParentLookup parentLookup)
    {
        Guard.Against(parentLookup is null, "ParentLookup must not be null.");
        Guard.Against(
            effectiveAt == default || effectiveAt == DateTimeOffset.MinValue || effectiveAt == DateTimeOffset.MaxValue,
            "Participant placement EffectiveAt must be a concrete, bounded DateTimeOffset.");

        var parentState = parentLookup!.GetState(homeCluster);
        if (parentState != StructuralParentState.Active)
            throw ParticipantErrors.InactiveParent(parentState);

        var aggregate = Register(id);
        aggregate.RaiseDomainEvent(new ParticipantPlacedEvent(id.Value, homeCluster, effectiveAt));
        return aggregate;
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ParticipantRegisteredEvent e:
                Id = new ParticipantId(e.ParticipantId);
                break;
            case ParticipantPlacedEvent e:
                HomeCluster = e.HomeCluster;
                PlacedAt = e.EffectiveAt;
                break;
        }
    }
}
