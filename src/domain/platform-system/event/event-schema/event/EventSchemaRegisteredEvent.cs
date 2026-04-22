using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public sealed record EventSchemaRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] EventSchemaId EventSchemaId,
    EventSchemaName Name,
    EventSchemaVersion Version,
    CompatibilityMode CompatibilityMode,
    Timestamp RegisteredAt) : DomainEvent;
