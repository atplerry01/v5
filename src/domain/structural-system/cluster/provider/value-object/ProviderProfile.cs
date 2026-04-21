using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public readonly record struct ProviderProfile
{
    public ClusterRef ClusterReference { get; }
    public string ProviderName { get; }

    public ProviderProfile(ClusterRef clusterReference, string providerName)
    {
        Guard.Against(clusterReference == default, "ProviderProfile cluster reference cannot be empty.");
        Guard.Against(string.IsNullOrWhiteSpace(providerName), "ProviderProfile name must not be null or whitespace.");

        ClusterReference = clusterReference;
        ProviderName = providerName;
    }
}
