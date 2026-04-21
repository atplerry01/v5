using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;

public sealed record CreateChannelCommand(
    Guid ChannelId,
    Guid StreamId,
    string Name,
    string Mode,
    DateTimeOffset CreatedAt) : IHasAggregateId
{
    public Guid AggregateId => ChannelId;
}

public sealed record RenameChannelCommand(
    Guid ChannelId,
    string NewName,
    DateTimeOffset RenamedAt) : IHasAggregateId
{
    public Guid AggregateId => ChannelId;
}

public sealed record EnableChannelCommand(
    Guid ChannelId,
    DateTimeOffset EnabledAt) : IHasAggregateId
{
    public Guid AggregateId => ChannelId;
}

public sealed record DisableChannelCommand(
    Guid ChannelId,
    string Reason,
    DateTimeOffset DisabledAt) : IHasAggregateId
{
    public Guid AggregateId => ChannelId;
}

public sealed record ArchiveChannelCommand(
    Guid ChannelId,
    DateTimeOffset ArchivedAt) : IHasAggregateId
{
    public Guid AggregateId => ChannelId;
}
