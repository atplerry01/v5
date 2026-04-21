using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class ClusterAggregate : AggregateRoot
{
    private readonly HashSet<Guid> _activeAuthorityIds = new();
    private readonly HashSet<Guid> _activeAdministrationIds = new();

    public ClusterId ClusterId { get; private set; }
    public ClusterDescriptor Descriptor { get; private set; }
    public ClusterStatus Status { get; private set; }

    public IReadOnlyCollection<ClusterAuthorityRef> ActiveAuthorities =>
        _activeAuthorityIds.Select(id => new ClusterAuthorityRef(id)).ToArray();

    public IReadOnlyCollection<ClusterAdministrationRef> ActiveAdministrations =>
        _activeAdministrationIds.Select(id => new ClusterAdministrationRef(id)).ToArray();

    public static ClusterAggregate Define(ClusterId id, ClusterDescriptor descriptor)
    {
        var aggregate = new ClusterAggregate();
        if (aggregate.Version >= 0)
            throw ClusterErrors.AlreadyInitialized();

        aggregate.RaiseDomainEvent(new ClusterDefinedEvent(id, descriptor));
        return aggregate;
    }

    public void Activate()
    {
        if (!new CanActivateSpecification().IsSatisfiedBy(Status))
            throw ClusterErrors.InvalidStateTransition(Status, nameof(Activate));

        RaiseDomainEvent(new ClusterActivatedEvent(ClusterId));
    }

    public void Archive()
    {
        if (!new CanArchiveSpecification().IsSatisfiedBy(Status))
            throw ClusterErrors.InvalidStateTransition(Status, nameof(Archive));

        RaiseDomainEvent(new ClusterArchivedEvent(ClusterId));
    }

    public void RecordAuthorityAttached(ClusterAuthorityRef authority)
    {
        var spec = new UniqueActiveAuthoritySpecification();
        if (!spec.IsSatisfiedBy(ActiveAuthorities, authority))
            throw ClusterErrors.DuplicateAuthority();

        RaiseDomainEvent(new ClusterAuthorityBoundEvent(ClusterId, authority));
    }

    public void RecordAuthorityReleased(ClusterAuthorityRef authority)
    {
        RaiseDomainEvent(new ClusterAuthorityReleasedEvent(ClusterId, authority));
    }

    public void RecordAdministrationAttached(ClusterAdministrationRef administration)
    {
        var spec = new UniqueAdministrationSpecification();
        if (!spec.IsSatisfiedBy(ActiveAdministrations, administration))
            throw ClusterErrors.DuplicateAdministration();

        RaiseDomainEvent(new ClusterAdministrationBoundEvent(ClusterId, administration));
    }

    public void RecordAdministrationReleased(ClusterAdministrationRef administration)
    {
        RaiseDomainEvent(new ClusterAdministrationReleasedEvent(ClusterId, administration));
    }

    protected override void Apply(object domainEvent)
    {
        switch (domainEvent)
        {
            case ClusterDefinedEvent e:
                ClusterId = e.ClusterId;
                Descriptor = e.Descriptor;
                Status = ClusterStatus.Defined;
                break;

            case ClusterActivatedEvent:
                Status = ClusterStatus.Active;
                break;

            case ClusterArchivedEvent:
                Status = ClusterStatus.Archived;
                break;

            case ClusterAuthorityBoundEvent b:
                _activeAuthorityIds.Add(b.Authority.Value);
                break;

            case ClusterAuthorityReleasedEvent r:
                _activeAuthorityIds.Remove(r.Authority.Value);
                break;

            case ClusterAdministrationBoundEvent b:
                _activeAdministrationIds.Add(b.Administration.Value);
                break;

            case ClusterAdministrationReleasedEvent r:
                _activeAdministrationIds.Remove(r.Administration.Value);
                break;
        }
    }

    protected override void EnsureInvariants()
    {
        if (ClusterId.Value == Guid.Empty)
            throw ClusterErrors.MissingId();

        if (string.IsNullOrWhiteSpace(Descriptor.ClusterName) ||
            string.IsNullOrWhiteSpace(Descriptor.ClusterType))
            throw ClusterErrors.MissingDescriptor();
    }
}
