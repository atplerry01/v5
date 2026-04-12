namespace Whycespace.Domain.BusinessSystem.Integration.Provider;

public sealed record ProviderCreatedEvent(ProviderId ProviderId, ProviderConfigId ConfigId, string ProviderName);
