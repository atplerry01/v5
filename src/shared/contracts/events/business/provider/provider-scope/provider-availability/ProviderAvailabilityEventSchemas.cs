namespace Whycespace.Shared.Contracts.Events.Business.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityCreatedEventSchema(
    Guid AggregateId,
    Guid ProviderId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record ProviderAvailabilityUpdatedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt);

public sealed record ProviderAvailabilityActivatedEventSchema(Guid AggregateId);

public sealed record ProviderAvailabilityArchivedEventSchema(Guid AggregateId);
