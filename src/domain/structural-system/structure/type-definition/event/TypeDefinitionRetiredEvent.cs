using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed record TypeDefinitionRetiredEvent(
    [property: JsonPropertyName("AggregateId")] TypeDefinitionId TypeDefinitionId) : DomainEvent;
