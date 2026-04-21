using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;

public sealed record CreatePlaybackCommand(
    Guid PlaybackId,
    Guid SourceId,
    string SourceKind,
    string Mode,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => PlaybackId;
}

public sealed record EnablePlaybackCommand(
    Guid PlaybackId,
    DateTimeOffset EnabledAt) : IHasAggregateId
{
    public Guid AggregateId => PlaybackId;
}

public sealed record DisablePlaybackCommand(
    Guid PlaybackId,
    string Reason,
    DateTimeOffset DisabledAt) : IHasAggregateId
{
    public Guid AggregateId => PlaybackId;
}

public sealed record UpdatePlaybackWindowCommand(
    Guid PlaybackId,
    DateTimeOffset AvailableFrom,
    DateTimeOffset AvailableUntil,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => PlaybackId;
}

public sealed record ArchivePlaybackCommand(
    Guid PlaybackId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => PlaybackId;
}
