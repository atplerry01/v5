using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.PlaybackConsumption.Session;

public sealed record OpenSessionCommand(
    Guid SessionId,
    Guid StreamId,
    DateTimeOffset OpenedAt,
    DateTimeOffset ExpiresAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record ActivateSessionCommand(
    Guid SessionId,
    DateTimeOffset ActivatedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record SuspendSessionCommand(
    Guid SessionId,
    DateTimeOffset SuspendedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record ResumeSessionCommand(
    Guid SessionId,
    DateTimeOffset ResumedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record CloseSessionCommand(
    Guid SessionId,
    string Reason,
    DateTimeOffset ClosedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record FailSessionCommand(
    Guid SessionId,
    string Reason,
    DateTimeOffset FailedAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}

public sealed record ExpireSessionCommand(
    Guid SessionId,
    DateTimeOffset ExpiredAt) : IHasAggregateId
{
    public Guid AggregateId => SessionId;
}
