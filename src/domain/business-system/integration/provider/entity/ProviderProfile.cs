namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed class ProviderProfile
{
    public ProviderConfigId ConfigId { get; }
    public string ProviderName { get; }
    public string ProviderType { get; }

    public ProviderProfile(ProviderConfigId configId, string providerName, string providerType)
    {
        if (configId == default)
            throw new ArgumentException("ConfigId must not be empty.", nameof(configId));

        if (string.IsNullOrWhiteSpace(providerName))
            throw new ArgumentException("ProviderName must not be empty.", nameof(providerName));

        if (string.IsNullOrWhiteSpace(providerType))
            throw new ArgumentException("ProviderType must not be empty.", nameof(providerType));

        ConfigId = configId;
        ProviderName = providerName;
        ProviderType = providerType;
    }
}
