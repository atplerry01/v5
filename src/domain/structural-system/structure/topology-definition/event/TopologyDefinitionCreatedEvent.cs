using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TopologyDefinition;

public sealed record TopologyDefinitionCreatedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId,
    TopologyDefinitionDescriptor Descriptor) : DomainEvent;

public sealed record TopologyDefinitionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId) : DomainEvent;

public sealed record TopologyDefinitionSuspendedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId) : DomainEvent;

public sealed record TopologyDefinitionReactivatedEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId) : DomainEvent;

public sealed record TopologyDefinitionRetiredEvent(
    [property: JsonPropertyName("AggregateId")] TopologyDefinitionId TopologyDefinitionId) : DomainEvent;
