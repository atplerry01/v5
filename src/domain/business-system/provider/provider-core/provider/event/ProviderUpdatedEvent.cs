namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.Provider;

public sealed record ProviderUpdatedEvent(ProviderId ProviderId, ProviderName Name, ProviderType Type);
