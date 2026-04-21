using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

using Whycespace.Domain.BusinessSystem.Shared.Time;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderScope.ProviderAvailability;

public sealed record ProviderAvailabilityCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderAvailabilityId ProviderAvailabilityId,
    ClusterProviderRef Provider,
    TimeWindow Window) : DomainEvent;
