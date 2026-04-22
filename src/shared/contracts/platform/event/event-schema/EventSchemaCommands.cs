using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Event.EventSchema;

public sealed record RegisterEventSchemaCommand(
    Guid EventSchemaId,
    string Name,
    string Version,
    string CompatibilityMode,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => EventSchemaId;
}

public sealed record DeprecateEventSchemaCommand(
    Guid EventSchemaId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => EventSchemaId;
}
