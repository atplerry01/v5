namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

using Whycespace.Domain.SharedKernel;
using Whycespace.Shared.Primitives.Id;

public sealed class FederationNode : Entity
{
    private FederationNode() { }

    public Guid PolicyId { get; private set; }
    public int Version { get; private set; }
    public string ClusterId { get; private set; } = default!;
    public FederationNodeStatus Status { get; private set; } = default!;

    public static FederationNode Create(Guid policyId, int version, string clusterId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(clusterId);

        return new FederationNode
        {
            Id = DeterministicIdHelper.FromSeed($"FederationNode:{policyId}:{version}:{clusterId}"),
            PolicyId = policyId,
            Version = version,
            ClusterId = clusterId,
            Status = FederationNodeStatus.Active
        };
    }
}
