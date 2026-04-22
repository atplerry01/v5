using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.HierarchyDefinition;

public sealed record HierarchyDefinitionLockedEvent(
    [property: JsonPropertyName("AggregateId")] HierarchyDefinitionId HierarchyDefinitionId) : DomainEvent;
