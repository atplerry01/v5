using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAvailabilityId ProviderAvailabilityId,
    TimeWindow Window) : DomainEvent;
