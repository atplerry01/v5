namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public readonly record struct AuthorityDescriptor
{
    public Guid ClusterReference { get; }
    public string AuthorityName { get; }

    public AuthorityDescriptor(Guid clusterReference, string authorityName)
    {
        if (clusterReference == Guid.Empty)
            throw AuthorityErrors.MissingDescriptor();

        if (string.IsNullOrWhiteSpace(authorityName))
            throw AuthorityErrors.MissingDescriptor();

        ClusterReference = clusterReference;
        AuthorityName = authorityName;
    }
}
