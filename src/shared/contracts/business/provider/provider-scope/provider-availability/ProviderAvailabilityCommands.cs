using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Business.Provider.ProviderScope.ProviderAvailability;

public sealed record CreateProviderAvailabilityCommand(
    Guid ProviderAvailabilityId,
    Guid ProviderId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAvailabilityId;
}

public sealed record UpdateProviderAvailabilityWindowCommand(
    Guid ProviderAvailabilityId,
    DateTimeOffset StartsAt,
    DateTimeOffset? EndsAt) : IHasAggregateId
{
    public Guid AggregateId => ProviderAvailabilityId;
}

public sealed record ActivateProviderAvailabilityCommand(Guid ProviderAvailabilityId) : IHasAggregateId
{
    public Guid AggregateId => ProviderAvailabilityId;
}

public sealed record ArchiveProviderAvailabilityCommand(Guid ProviderAvailabilityId) : IHasAggregateId
{
    public Guid AggregateId => ProviderAvailabilityId;
}
