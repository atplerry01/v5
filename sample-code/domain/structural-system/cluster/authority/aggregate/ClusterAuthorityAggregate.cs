using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class ClusterAuthorityAggregate : AggregateRoot
{
    public Guid ClusterId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private readonly List<Guid> _subClusters = new();
    public IReadOnlyList<Guid> SubClusters => _subClusters;

    public ClusterAuthorityAggregate() { }

    public static ClusterAuthorityAggregate Create(
        Guid id,
        Guid clusterId,
        string name)
    {
        if (id == Guid.Empty)
            throw new AuthorityException("Authority id required");

        if (clusterId == Guid.Empty)
            throw new AuthorityException("Cluster required");

        if (string.IsNullOrWhiteSpace(name))
            throw new AuthorityException("Authority name required");

        var aggregate = new ClusterAuthorityAggregate
        {
            Id = id,
            ClusterId = clusterId,
            Name = name
        };

        aggregate.RaiseDomainEvent(new AuthorityCreatedEvent(id));

        return aggregate;
    }

    public void AddSubCluster(Guid subClusterId)
    {
        if (subClusterId == Guid.Empty)
            throw new AuthorityException("SubCluster id required");

        if (_subClusters.Contains(subClusterId))
            throw new AuthorityException("SubCluster already exists in this authority");

        _subClusters.Add(subClusterId);

        RaiseDomainEvent(new AuthoritySubClusterAddedEvent(Id, subClusterId));
    }
}
