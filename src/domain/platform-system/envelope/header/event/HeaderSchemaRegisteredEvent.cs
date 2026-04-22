using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Envelope.Header;

public sealed record HeaderSchemaRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] HeaderSchemaId HeaderSchemaId,
    HeaderKind HeaderKind,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    Timestamp RegisteredAt) : DomainEvent;
