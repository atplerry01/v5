using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Document.Descriptor.Metadata;

public sealed record CreateDocumentMetadataCommand(
    Guid MetadataId,
    Guid DocumentId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record AddDocumentMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    string Value,
    DateTimeOffset AddedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record UpdateDocumentMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    string NewValue,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record RemoveDocumentMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record FinalizeDocumentMetadataCommand(
    Guid MetadataId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}
