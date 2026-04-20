namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierCreatedEvent(
    ProviderTierId ProviderTierId,
    TierCode Code,
    TierName Name,
    TierRank Rank);
