using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;

public sealed record CreateTranscriptCommand(
    Guid TranscriptId,
    Guid AssetRef,
    Guid? SourceFileRef,
    string Format,
    string Language,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => TranscriptId;
}

public sealed record UpdateTranscriptCommand(
    Guid TranscriptId,
    Guid OutputRef,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => TranscriptId;
}

public sealed record FinalizeTranscriptCommand(
    Guid TranscriptId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => TranscriptId;
}

public sealed record ArchiveTranscriptCommand(
    Guid TranscriptId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => TranscriptId;
}
