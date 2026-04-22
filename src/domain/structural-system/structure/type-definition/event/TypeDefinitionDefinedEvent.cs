using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.StructuralSystem.Structure.TypeDefinition;

public sealed record TypeDefinitionDefinedEvent(
    [property: JsonPropertyName("AggregateId")] TypeDefinitionId TypeDefinitionId,
    TypeDefinitionDescriptor Descriptor) : DomainEvent;
