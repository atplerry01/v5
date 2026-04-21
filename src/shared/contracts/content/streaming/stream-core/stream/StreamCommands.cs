using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Stream;

public sealed record CreateStreamCommand(
    Guid StreamId,
    string Mode,
    string Type,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}

public sealed record ActivateStreamCommand(
    Guid StreamId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}

public sealed record PauseStreamCommand(
    Guid StreamId,
    DateTimeOffset PausedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}

public sealed record ResumeStreamCommand(
    Guid StreamId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}

public sealed record EndStreamCommand(
    Guid StreamId,
    DateTimeOffset EndedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}

public sealed record ArchiveStreamCommand(
    Guid StreamId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => StreamId;
}
