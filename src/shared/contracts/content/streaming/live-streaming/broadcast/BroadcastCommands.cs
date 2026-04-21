using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Broadcast;

public sealed record CreateBroadcastCommand(
    Guid BroadcastId,
    Guid StreamId,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record ScheduleBroadcastCommand(
    Guid BroadcastId,
    DateTimeOffset ScheduledStart,
    DateTimeOffset ScheduledEnd,
    DateTimeOffset ScheduledAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record StartBroadcastCommand(
    Guid BroadcastId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record PauseBroadcastCommand(
    Guid BroadcastId,
    DateTimeOffset PausedAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record ResumeBroadcastCommand(
    Guid BroadcastId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record EndBroadcastCommand(
    Guid BroadcastId,
    DateTimeOffset EndedAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}

public sealed record CancelBroadcastCommand(
    Guid BroadcastId,
    string Reason,
    DateTimeOffset CancelledAt) : IHasAggregateId
{
    public Guid AggregateId => BroadcastId;
}
