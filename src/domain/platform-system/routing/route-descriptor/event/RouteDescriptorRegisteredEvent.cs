using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Routing.RouteDescriptor;

public sealed record RouteDescriptorRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] RouteDescriptorId RouteDescriptorId,
    DomainRoute Source,
    DomainRoute Destination,
    string TransportHint,
    int Priority,
    Timestamp RegisteredAt) : DomainEvent;
