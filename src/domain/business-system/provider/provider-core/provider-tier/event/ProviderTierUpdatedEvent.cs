namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierUpdatedEvent(ProviderTierId ProviderTierId, TierName Name, TierRank Rank);
