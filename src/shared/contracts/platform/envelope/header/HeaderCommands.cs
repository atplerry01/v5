using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Envelope.Header;

public sealed record RegisterHeaderSchemaCommand(
    Guid HeaderSchemaId,
    string HeaderKind,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => HeaderSchemaId;
}

public sealed record DeprecateHeaderSchemaCommand(
    Guid HeaderSchemaId,
    DateTimeOffset DeprecatedAt) : IHasAggregateId
{
    public Guid AggregateId => HeaderSchemaId;
}
