using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public sealed record TopologyDefinitionCreatedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId,
    TopologyDefinitionDescriptor Descriptor) : DomainEvent;
