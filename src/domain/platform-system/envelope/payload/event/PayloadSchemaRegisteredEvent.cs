using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public sealed record PayloadSchemaRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] PayloadSchemaId PayloadSchemaId,
    string TypeRef,
    PayloadEncoding Encoding,
    string? SchemaRef,
    int SchemaContractVersion,
    long? MaxSizeBytes,
    Timestamp RegisteredAt) : DomainEvent;
