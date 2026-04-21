namespace Whycespace.Shared.Contracts.Events.Business.Provider.ProviderCore.ProviderTier;

public sealed record ProviderTierCreatedEventSchema(
    Guid AggregateId,
    string Code,
    string Name,
    int Rank);

public sealed record ProviderTierUpdatedEventSchema(
    Guid AggregateId,
    string Name,
    int Rank);

public sealed record ProviderTierActivatedEventSchema(Guid AggregateId);

public sealed record ProviderTierArchivedEventSchema(Guid AggregateId);
