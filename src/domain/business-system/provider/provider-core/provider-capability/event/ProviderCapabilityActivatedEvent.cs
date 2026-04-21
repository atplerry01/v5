using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityActivatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCapabilityId ProviderCapabilityId) : DomainEvent;
