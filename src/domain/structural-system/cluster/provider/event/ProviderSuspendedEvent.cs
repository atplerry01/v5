using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Provider;

public sealed record ProviderSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] ProviderId ProviderId) : DomainEvent;
