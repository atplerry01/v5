using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityUpdatedEvent(
    ProviderAvailabilityId ProviderAvailabilityId,
    TimeWindow Window);
