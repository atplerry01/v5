using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Replay;

public sealed record RequestReplayCommand(
    Guid ReplayId,
    Guid ArchiveId,
    Guid ViewerId) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}

public sealed record StartReplayCommand(
    Guid ReplayId,
    long PositionMs,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}

public sealed record PauseReplayCommand(
    Guid ReplayId,
    long PositionMs,
    DateTimeOffset PausedAt) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}

public sealed record ResumeReplayCommand(
    Guid ReplayId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}

public sealed record CompleteReplayCommand(
    Guid ReplayId,
    long PositionMs,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}

public sealed record AbandonReplayCommand(
    Guid ReplayId,
    DateTimeOffset AbandonedAt) : IHasAggregateId
{
    public Guid AggregateId => ReplayId;
}
