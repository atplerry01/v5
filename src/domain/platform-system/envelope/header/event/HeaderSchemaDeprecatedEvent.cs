using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public sealed record HeaderSchemaDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] HeaderSchemaId HeaderSchemaId,
    Timestamp DeprecatedAt) : DomainEvent;
