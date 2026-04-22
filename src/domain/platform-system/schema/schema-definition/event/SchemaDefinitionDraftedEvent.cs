using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.SchemaDefinition;

public sealed record SchemaDefinitionDraftedEvent(
    [property: JsonPropertyName("AggregateId")] SchemaDefinitionId SchemaDefinitionId,
    SchemaName SchemaName,
    int Version,
    IReadOnlyList<FieldDescriptor> Fields,
    SchemaCompatibilityMode CompatibilityMode,
    Timestamp DraftedAt) : DomainEvent;
