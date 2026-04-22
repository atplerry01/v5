using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvCreatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId,
    SpvDescriptor Descriptor) : DomainEvent;
