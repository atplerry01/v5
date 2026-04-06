using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Topology;

public sealed class SubClusterAggregate : AggregateRoot
{
    public Guid AuthorityId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private readonly List<Guid> _spvs = new();
    public IReadOnlyList<Guid> Spvs => _spvs;

    public SubClusterAggregate() { }

    public static SubClusterAggregate Create(
        Guid id,
        Guid authorityId,
        string name)
    {
        if (id == Guid.Empty)
            throw new SubClusterException("SubCluster id required");

        if (authorityId == Guid.Empty)
            throw new SubClusterException("Authority required");

        if (string.IsNullOrWhiteSpace(name))
            throw new SubClusterException("SubCluster name required");

        var aggregate = new SubClusterAggregate
        {
            Id = id,
            AuthorityId = authorityId,
            Name = name
        };

        aggregate.RaiseDomainEvent(new SubClusterCreatedEvent(id));

        return aggregate;
    }

    public void AddSpv(Guid spvId)
    {
        if (spvId == Guid.Empty)
            throw new SubClusterException("SPV id required");

        if (_spvs.Contains(spvId))
            throw new SubClusterException("SPV already exists in this subcluster");

        _spvs.Add(spvId);

        RaiseDomainEvent(new SubClusterSpvAddedEvent(Id, spvId));
    }
}
