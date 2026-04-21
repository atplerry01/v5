namespace Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Channel;

public sealed record ChannelCreatedEventSchema(
    Guid AggregateId,
    Guid StreamId,
    string Name,
    string Mode,
    DateTimeOffset CreatedAt);

public sealed record ChannelRenamedEventSchema(
    Guid AggregateId,
    string PreviousName,
    string NewName,
    DateTimeOffset RenamedAt);

public sealed record ChannelEnabledEventSchema(
    Guid AggregateId,
    DateTimeOffset EnabledAt);

public sealed record ChannelDisabledEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset DisabledAt);

public sealed record ChannelArchivedEventSchema(
    Guid AggregateId,
    DateTimeOffset ArchivedAt);
