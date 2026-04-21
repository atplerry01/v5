using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityUpdatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCapabilityId ProviderCapabilityId,
    CapabilityName Name) : DomainEvent;
