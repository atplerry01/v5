using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public sealed record TopologyDefinitionRetiredEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId) : DomainEvent;
