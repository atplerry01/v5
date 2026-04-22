using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed record TypeDefinitionActivatedEvent(
    [property: JsonPropertyName("AggregateId")] TypeDefinitionId TypeDefinitionId) : DomainEvent;
