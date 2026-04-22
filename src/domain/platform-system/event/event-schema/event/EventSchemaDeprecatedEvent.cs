using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventSchema;

public sealed record EventSchemaDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] EventSchemaId EventSchemaId,
    Timestamp DeprecatedAt) : DomainEvent;
