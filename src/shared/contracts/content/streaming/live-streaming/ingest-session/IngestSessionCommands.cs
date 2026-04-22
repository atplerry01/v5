using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.LiveStreaming.IngestSession;

public sealed record AuthenticateIngestSessionCommand(
    Guid SessionId,
    Guid BroadcastId,
    string Endpoint) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record StartIngestStreamingCommand(
    Guid SessionId,
    DateTimeOffset StartedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record StallIngestSessionCommand(
    Guid SessionId,
    DateTimeOffset StalledAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record ResumeIngestSessionCommand(
    Guid SessionId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record EndIngestSessionCommand(
    Guid SessionId,
    DateTimeOffset EndedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record FailIngestSessionCommand(
    Guid SessionId,
    string FailureReason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}
