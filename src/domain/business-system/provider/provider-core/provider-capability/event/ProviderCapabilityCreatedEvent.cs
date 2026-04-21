using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;
using Whycespace.Domain.StructuralSystem.Contracts.References;

namespace Whycespace.Domain.BusinessSystem.Provider.ProviderCore.ProviderCapability;

public sealed record ProviderCapabilityCreatedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderCapabilityId ProviderCapabilityId,
    ClusterProviderRef Provider,
    CapabilityCode Code,
    CapabilityName Name) : DomainEvent;
