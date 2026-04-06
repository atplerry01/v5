namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;

public sealed class ClusterBoundary : ValueObject
{
    public string ClusterId { get; }
    public IReadOnlyList<Guid> PolicyIds { get; }

    private ClusterBoundary(string clusterId, IReadOnlyList<Guid> policyIds)
    {
        ClusterId = clusterId;
        PolicyIds = policyIds;
    }

    public static ClusterBoundary Create(string clusterId, IReadOnlyList<Guid> policyIds)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterId);
        ArgumentNullException.ThrowIfNull(policyIds);
        return new ClusterBoundary(clusterId, policyIds);
    }

    public bool Contains(Guid policyId) => PolicyIds.Contains(policyId);

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return ClusterId;
        foreach (var id in PolicyIds.OrderBy(x => x)) yield return id;
    }
}
