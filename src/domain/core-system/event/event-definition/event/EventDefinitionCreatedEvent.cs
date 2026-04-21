using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Event.EventDefinition;

public sealed record EventDefinitionRegisteredEvent(
    [property: JsonPropertyName("AggregateId")] EventDefinitionId DefinitionId,
    EventSchema Schema) : DomainEvent;
