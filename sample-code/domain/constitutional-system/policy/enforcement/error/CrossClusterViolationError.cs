using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.ConstitutionalSystem.Policy.Registry;

public sealed class CrossClusterViolationError : DomainException
{
    public Guid SourcePolicyId { get; }
    public Guid TargetPolicyId { get; }
    public string SourceCluster { get; }
    public string TargetCluster { get; }

    public CrossClusterViolationError(
        Guid sourcePolicyId,
        Guid targetPolicyId,
        string sourceCluster,
        string targetCluster)
        : base(
            "CROSS_CLUSTER_VIOLATION",
            $"Cross-cluster violation: policy {sourcePolicyId} ({sourceCluster}) cannot reference policy {targetPolicyId} ({targetCluster}).")
    {
        SourcePolicyId = sourcePolicyId;
        TargetPolicyId = targetPolicyId;
        SourceCluster = sourceCluster;
        TargetCluster = targetCluster;
    }
}
