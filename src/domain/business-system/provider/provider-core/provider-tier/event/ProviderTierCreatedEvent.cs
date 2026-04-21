using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderTierId ProviderTierId,
    TierCode Code,
    TierName Name,
    TierRank Rank) : DomainEvent;
