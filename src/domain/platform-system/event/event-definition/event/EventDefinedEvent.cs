using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.PlatformSystem.Event.EventDefinition;

public sealed record EventDefinedEvent(
    [property: JsonPropertyName("AggregateId")] EventDefinitionId EventDefinitionId,
    EventTypeName TypeName,
    EventVersion Version,
    string SchemaId,
    DomainRoute SourceRoute,
    Timestamp DefinedAt) : DomainEvent;
