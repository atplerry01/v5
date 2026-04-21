using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.Descriptor.Metadata;

public sealed record CreateMediaMetadataCommand(
    Guid MetadataId,
    Guid AssetRef,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record AddMediaMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    string Value,
    DateTimeOffset AddedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record UpdateMediaMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    string NewValue,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record RemoveMediaMetadataEntryCommand(
    Guid MetadataId,
    string Key,
    DateTimeOffset RemovedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}

public sealed record FinalizeMediaMetadataCommand(
    Guid MetadataId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => MetadataId;
}
