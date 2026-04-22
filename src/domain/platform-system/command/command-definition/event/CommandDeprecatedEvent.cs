using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Command.CommandDefinition;

public sealed record CommandDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] CommandDefinitionId CommandDefinitionId,
    CommandTypeName TypeName,
    CommandVersion Version,
    Timestamp DeprecatedAt) : DomainEvent;
