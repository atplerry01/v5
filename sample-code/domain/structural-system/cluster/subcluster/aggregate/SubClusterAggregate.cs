namespace Whycespace.Domain.StructuralSystem.Cluster.SubCluster;

public sealed class SubClusterAggregate : AggregateRoot
{
    public Guid AuthorityId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public SubClusterStatus Status { get; private set; } = SubClusterStatus.Created;

    private readonly HashSet<Guid> _spvIds = [];
    public IReadOnlyCollection<Guid> SpvIds => _spvIds;

    public SubClusterAggregate() { }

    public static SubClusterAggregate Create(
        Guid id,
        Guid authorityId,
        string name)
    {
        Guard.AgainstDefault(id);
        Guard.AgainstDefault(authorityId);
        Guard.AgainstEmpty(name);

        var aggregate = new SubClusterAggregate
        {
            Id = id,
            AuthorityId = authorityId,
            Name = name,
            Status = SubClusterStatus.Created
        };

        aggregate.RaiseDomainEvent(new SubClusterCreatedEvent(id, authorityId, name));
        return aggregate;
    }

    public void Activate()
    {
        EnsureInvariant(
            Status == SubClusterStatus.Created,
            "SUBCLUSTER_MUST_BE_CREATED",
            "Only created subclusters can be activated.");

        Status = SubClusterStatus.Active;
        RaiseDomainEvent(new SubClusterActivatedEvent(Id));
    }

    public void Deactivate(string reason)
    {
        EnsureInvariant(
            Status == SubClusterStatus.Active,
            "SUBCLUSTER_MUST_BE_ACTIVE",
            "Only active subclusters can be deactivated.");

        Status = SubClusterStatus.Deactivated;
        RaiseDomainEvent(new SubClusterDeactivatedEvent(Id, reason));
    }

    public void AddSpv(Guid spvId)
    {
        Guard.AgainstDefault(spvId);
        EnsureInvariant(
            Status == SubClusterStatus.Active,
            "SUBCLUSTER_MUST_BE_ACTIVE",
            "Only active subclusters can add SPVs.");

        if (!_spvIds.Add(spvId))
            throw new SubClusterException("SPV already exists in this subcluster");

        RaiseDomainEvent(new SubClusterSpvAddedEvent(Id, spvId));
    }

    public void RemoveSpv(Guid spvId)
    {
        EnsureInvariant(
            _spvIds.Contains(spvId),
            "SPV_NOT_FOUND",
            $"SPV '{spvId}' not found in this subcluster.");

        _spvIds.Remove(spvId);
        RaiseDomainEvent(new SubClusterSpvRemovedEvent(Id, spvId));
    }
}
