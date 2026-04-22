using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Schema.Serialization;

public sealed record SerializationFormatDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] SerializationFormatId SerializationFormatId,
    Timestamp DeprecatedAt) : DomainEvent;
