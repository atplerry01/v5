using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Cluster;

public sealed class WhyceClusterAggregate : AggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Jurisdiction { get; private set; } = string.Empty;
    public ClusterStatus Status { get; private set; } = ClusterStatus.Created;

    private readonly List<Guid> _authorities = new();
    public IReadOnlyList<Guid> Authorities => _authorities;

    public WhyceClusterAggregate() { }

    public static WhyceClusterAggregate Create(
        Guid id,
        string name,
        string jurisdiction)
    {
        if (id == Guid.Empty)
            throw new ClusterException("Cluster id required");

        if (string.IsNullOrWhiteSpace(name))
            throw new ClusterException("Cluster name required");

        if (string.IsNullOrWhiteSpace(jurisdiction))
            throw new ClusterException("Cluster jurisdiction required");

        var aggregate = new WhyceClusterAggregate
        {
            Id = id,
            Name = name,
            Jurisdiction = jurisdiction,
            Status = ClusterStatus.Created
        };

        aggregate.RaiseDomainEvent(new ClusterCreatedEvent(id));

        return aggregate;
    }

    public void Activate()
    {
        if (Status != ClusterStatus.Created)
            throw new ClusterException("Invalid transition: only Created clusters can be activated");

        Status = ClusterStatus.Active;

        RaiseDomainEvent(new ClusterActivatedEvent(Id));
    }

    public void AddAuthority(Guid authorityId)
    {
        if (authorityId == Guid.Empty)
            throw new ClusterException("Authority id required");

        if (_authorities.Contains(authorityId))
            throw new ClusterException("Authority already exists in this cluster");

        _authorities.Add(authorityId);

        RaiseDomainEvent(new ClusterAuthorityAddedEvent(Id, authorityId));
    }
}
