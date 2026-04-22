using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Progress;

public sealed record TrackProgressCommand(
    Guid ProgressId,
    Guid SessionId,
    long PositionMs) : IHasAggregateId
{
    public Guid AggregateId => ProgressId;
}

public sealed record UpdatePlaybackPositionCommand(
    Guid ProgressId,
    long PositionMs,
    DateTimeOffset UpdatedAt) : IHasAggregateId
{
    public Guid AggregateId => ProgressId;
}

public sealed record PausePlaybackCommand(
    Guid ProgressId,
    long PositionMs,
    DateTimeOffset PausedAt) : IHasAggregateId
{
    public Guid AggregateId => ProgressId;
}

public sealed record ResumePlaybackCommand(
    Guid ProgressId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => ProgressId;
}

public sealed record TerminateProgressCommand(
    Guid ProgressId,
    DateTimeOffset TerminatedAt) : IHasAggregateId
{
    public Guid AggregateId => ProgressId;
}
