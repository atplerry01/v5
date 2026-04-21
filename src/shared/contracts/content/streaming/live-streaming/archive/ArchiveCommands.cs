using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.Archive;

public sealed record StartArchiveCommand(
    Guid ArchiveId,
    Guid StreamId,
    Guid? SessionId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => ArchiveId;
}

public sealed record CompleteArchiveCommand(
    Guid ArchiveId,
    Guid OutputId,
    DateTimeOffset CompletedAt) : IHasAggregateId
{
    public Guid AggregateId => ArchiveId;
}

public sealed record FailArchiveCommand(
    Guid ArchiveId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => ArchiveId;
}

public sealed record FinalizeArchiveCommand(
    Guid ArchiveId,
    DateTimeOffset FinalizedAt) : IHasAggregateId
{
    public Guid AggregateId => ArchiveId;
}

public sealed record ArchiveArchiveCommand(
    Guid ArchiveId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => ArchiveId;
}
