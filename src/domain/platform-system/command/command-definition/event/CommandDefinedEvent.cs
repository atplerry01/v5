using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public sealed record CommandDefinedEvent(
    [property: JsonPropertyName("AggregateId")] CommandDefinitionId CommandDefinitionId,
    CommandTypeName TypeName,
    CommandVersion Version,
    string SchemaId,
    DomainRoute OwnerRoute) : DomainEvent;
