using Whycespace.Domain.BusinessSystem.Shared.Reference;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityCreatedEvent(
    ProviderAvailabilityId ProviderAvailabilityId,
    ProviderRef Provider,
    TimeWindow Window);
