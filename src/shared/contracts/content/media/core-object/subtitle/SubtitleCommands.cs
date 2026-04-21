using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;

public sealed record CreateSubtitleCommand(
    Guid SubtitleId,
    Guid AssetRef,
    Guid? SourceFileRef,
    string Format,
    string Language,
    long? WindowStartMs,
    long? WindowEndMs,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => SubtitleId;
}

public sealed record UpdateSubtitleCommand(
    Guid SubtitleId,
    Guid OutputRef,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => SubtitleId;
}

public sealed record FinalizeSubtitleCommand(
    Guid SubtitleId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => SubtitleId;
}

public sealed record ArchiveSubtitleCommand(
    Guid SubtitleId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => SubtitleId;
}
