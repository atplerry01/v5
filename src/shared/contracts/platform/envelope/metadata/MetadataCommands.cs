using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Platform.Envelope.Metadata;

public sealed record RegisterMessageMetadataSchemaCommand(
    Guid MetadataSchemaId,
    int SchemaVersion,
    IReadOnlyList<string> RequiredFields,
    IReadOnlyList<string> OptionalFields,
    DateTimeOffset RegisteredAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataSchemaId;
}
