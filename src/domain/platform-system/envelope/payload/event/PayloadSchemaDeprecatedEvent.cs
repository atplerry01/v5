using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Payload;

public sealed record PayloadSchemaDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] PayloadSchemaId PayloadSchemaId,
    Timestamp DeprecatedAt) : DomainEvent;
