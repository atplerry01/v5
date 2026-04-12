namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class ClusterAggregate
{
    private readonly List<object> _uncommittedEvents = new();

    public ClusterId ClusterId { get; private set; }
    public ClusterDescriptor Descriptor { get; private set; }
    public ClusterStatus Status { get; private set; }

    public IReadOnlyList<object> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    private ClusterAggregate() { }

    public static ClusterAggregate Define(ClusterId id, ClusterDescriptor descriptor)
    {
        var aggregate = new ClusterAggregate();
        aggregate.Apply(new ClusterDefinedEvent(id, descriptor));
        aggregate.EnsureInvariants();
        return aggregate;
    }

    public void Activate()
    {
        if (!new CanActivateSpecification().IsSatisfiedBy(Status))
            throw ClusterErrors.InvalidStateTransition(Status, nameof(Activate));

        Apply(new ClusterActivatedEvent(ClusterId));
        EnsureInvariants();
    }

    public void Archive()
    {
        if (!new CanArchiveSpecification().IsSatisfiedBy(Status))
            throw ClusterErrors.InvalidStateTransition(Status, nameof(Archive));

        Apply(new ClusterArchivedEvent(ClusterId));
        EnsureInvariants();
    }

    private void Apply(object domainEvent)
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
        }

        _uncommittedEvents.Add(domainEvent);
    }

    private void EnsureInvariants()
    {
        if (ClusterId.Value == Guid.Empty)
            throw ClusterErrors.MissingId();

        if (string.IsNullOrWhiteSpace(Descriptor.ClusterName) ||
            string.IsNullOrWhiteSpace(Descriptor.ClusterType))
            throw ClusterErrors.MissingDescriptor();
    }
}
