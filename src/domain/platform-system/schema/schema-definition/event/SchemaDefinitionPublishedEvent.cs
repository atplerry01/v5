using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public sealed record SchemaDefinitionPublishedEvent(
    [property: JsonPropertyName("AggregateId")] SchemaDefinitionId SchemaDefinitionId,
    Timestamp PublishedAt) : DomainEvent;
