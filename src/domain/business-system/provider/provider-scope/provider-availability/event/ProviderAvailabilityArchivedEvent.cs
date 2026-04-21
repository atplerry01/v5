using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityArchivedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAvailabilityId ProviderAvailabilityId) : DomainEvent;
