using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public sealed record HierarchyDefinitionDefinedEvent(
    [property: JsonPropertyName("AggregateId")] HierarchyDefinitionId HierarchyDefinitionId,
    HierarchyDefinitionDescriptor Descriptor) : DomainEvent;
