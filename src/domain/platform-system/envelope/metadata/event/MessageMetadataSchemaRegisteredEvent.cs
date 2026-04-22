using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Metadata;

public sealed record MessageMetadataSchemaRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] MetadataSchemaId MetadataSchemaId,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> OptionalFields,
    Timestamp RegisteredAt) : DomainEvent;
