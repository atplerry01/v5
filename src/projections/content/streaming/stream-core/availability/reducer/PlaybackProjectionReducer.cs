using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Availability;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Availability;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Availability.Reducer;

public static class PlaybackProjectionReducer
{
    public static PlaybackReadModel Apply(PlaybackReadModel state, PlaybackCreatedEventSchema e) =>
        state with
        {
            PlaybackId = e.AggregateId,
            SourceId = e.SourceId,
            SourceKind = e.SourceKind,
            Mode = e.Mode,
            AvailableFrom = e.AvailableFrom,
            AvailableUntil = e.AvailableUntil,
            Status = "Created",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static PlaybackReadModel Apply(PlaybackReadModel state, PlaybackEnabledEventSchema e) =>
        state with { PlaybackId = e.AggregateId, Status = "Enabled", LastModifiedAt = e.EnabledAt };

    public static PlaybackReadModel Apply(PlaybackReadModel state, PlaybackDisabledEventSchema e) =>
        state with { PlaybackId = e.AggregateId, Status = "Disabled", LastModifiedAt = e.DisabledAt };

    public static PlaybackReadModel Apply(PlaybackReadModel state, PlaybackWindowUpdatedEventSchema e) =>
        state with
        {
            PlaybackId = e.AggregateId,
            AvailableFrom = e.NewAvailableFrom,
            AvailableUntil = e.NewAvailableUntil,
            LastModifiedAt = e.UpdatedAt
        };

    public static PlaybackReadModel Apply(PlaybackReadModel state, PlaybackArchivedEventSchema e) =>
        state with { PlaybackId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
