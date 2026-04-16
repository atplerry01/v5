using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.Asset;

public sealed record RegisterMediaAssetCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    Guid AssetId,
    string OwnerRef,
    int MediaType,
    string Title,
    string Description,
    string ContentDigest,
    string StorageUri,
    long StorageSizeBytes,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record StartMediaAssetProcessingCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record MarkMediaAssetAvailableCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record PublishMediaAssetCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record UnpublishMediaAssetCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record ArchiveMediaAssetCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}

public sealed record UpdateMediaAssetMetadataCommand(
    Guid CommandId,
    Guid AggregateId,
    Guid CorrelationId,
    Guid CausationId,
    string Title,
    string Description,
    IReadOnlyList<string> Tags,
    DateTimeOffset OccurredAt) : IHasAggregateId
{
    public Guid AggregateId => CommandId;
}
