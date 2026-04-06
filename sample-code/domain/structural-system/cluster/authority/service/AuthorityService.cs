namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public sealed class AuthorityService
{
    public bool CanDelegateAuthority(
        ClusterAuthorityAggregate authority,
        Guid targetSubClusterId)
    {
        return !authority.SubClusters.Contains(targetSubClusterId);
    }

    public bool RequiresMultiSignature(
        ClusterAuthorityAggregate authority,
        string operationType)
    {
        return operationType switch
        {
            "CreateSubCluster" => authority.SubClusters.Count >= 3,
            "RemoveSubCluster" => true,
            "TransferAuthority" => true,
            _ => false
        };
    }
}
