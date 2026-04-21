using System.Text.Json.Serialization;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.CoreSystem.Command.CommandDefinition;

public sealed record CommandDefinitionDeprecatedEvent(
    [property: JsonPropertyName("AggregateId")] CommandDefinitionId DefinitionId) : DomainEvent;
