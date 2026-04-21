using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Channel;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Channel;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Channel.Reducer;

public static class ChannelProjectionReducer
{
    public static ChannelReadModel Apply(ChannelReadModel state, ChannelCreatedEventSchema e) =>
        state with
        {
            ChannelId = e.AggregateId,
            StreamId = e.StreamId,
            Name = e.Name,
            Mode = e.Mode,
            Status = "Created",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static ChannelReadModel Apply(ChannelReadModel state, ChannelRenamedEventSchema e) =>
        state with { ChannelId = e.AggregateId, Name = e.NewName, LastModifiedAt = e.RenamedAt };

    public static ChannelReadModel Apply(ChannelReadModel state, ChannelEnabledEventSchema e) =>
        state with { ChannelId = e.AggregateId, Status = "Enabled", LastModifiedAt = e.EnabledAt };

    public static ChannelReadModel Apply(ChannelReadModel state, ChannelDisabledEventSchema e) =>
        state with { ChannelId = e.AggregateId, Status = "Disabled", LastModifiedAt = e.DisabledAt };

    public static ChannelReadModel Apply(ChannelReadModel state, ChannelArchivedEventSchema e) =>
        state with { ChannelId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
