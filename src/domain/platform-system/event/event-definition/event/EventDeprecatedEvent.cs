using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public sealed record EventDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] EventDefinitionId EventDefinitionId,
    Timestamp DeprecatedAt) : DomainEvent;
