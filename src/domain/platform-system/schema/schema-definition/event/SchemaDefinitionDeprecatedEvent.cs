using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public sealed record SchemaDefinitionDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] SchemaDefinitionId SchemaDefinitionId,
    Timestamp DeprecatedAt) : DomainEvent;
