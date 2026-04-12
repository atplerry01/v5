namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public readonly record struct ProviderProfile
{
    public Guid ClusterReference { get; }
    public string ProviderName { get; }

    public ProviderProfile(Guid clusterReference, string providerName)
    {
        if (clusterReference == Guid.Empty)
            throw ProviderErrors.MissingProfile();

        if (string.IsNullOrWhiteSpace(providerName))
            throw ProviderErrors.MissingProfile();

        ClusterReference = clusterReference;
        ProviderName = providerName;
    }
}
