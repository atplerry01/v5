using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Authority;

public readonly record struct AuthorityDescriptor
{
    public ClusterRef ClusterReference { get; }
    public string AuthorityName { get; }

    public AuthorityDescriptor(ClusterRef clusterReference, string authorityName)
    {
        Guard.Against(clusterReference == default, "AuthorityDescriptor cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(authorityName), "AuthorityDescriptor name must not be null or whitespace.");

        ClusterReference = clusterReference;
        AuthorityName = authorityName;
    }
}
