using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Cluster.Spv;

public sealed record SpvRetiredEvent(
    [property: JsonPropertyName("AggregateId")] SpvId SpvId) : DomainEvent;
