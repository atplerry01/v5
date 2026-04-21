using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed record ProviderRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] ProviderId ProviderId,
    ProviderProfile Profile) : DomainEvent;
