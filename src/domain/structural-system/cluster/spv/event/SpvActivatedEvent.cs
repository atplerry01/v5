using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvActivatedEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;
