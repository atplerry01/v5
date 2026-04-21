using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderTierId ProviderTierId,
    TierName Name,
    TierRank Rank) : DomainEvent;
