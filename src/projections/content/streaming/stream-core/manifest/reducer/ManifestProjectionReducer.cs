using Whycespace.Shared.Contracts.Content.Streaming.StreamCore.Manifest;
using Whycespace.Shared.Contracts.Events.Content.Streaming.StreamCore.Manifest;

namespace Whycespace.Projections.Content.Streaming.StreamCore.Manifest.Reducer;

public static class ManifestProjectionReducer
{
    public static ManifestReadModel Apply(ManifestReadModel state, ManifestCreatedEventSchema e) =>
        state with
        {
            ManifestId = e.AggregateId,
            SourceId = e.SourceId,
            SourceKind = e.SourceKind,
            CurrentVersion = e.InitialVersion,
            Status = "Created",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static ManifestReadModel Apply(ManifestReadModel state, ManifestUpdatedEventSchema e) =>
        state with { ManifestId = e.AggregateId, CurrentVersion = e.NewVersion, LastModifiedAt = e.UpdatedAt };

    public static ManifestReadModel Apply(ManifestReadModel state, ManifestPublishedEventSchema e) =>
        state with
        {
            ManifestId = e.AggregateId,
            Status = "Published",
            PublishedAt = state.PublishedAt ?? e.PublishedAt,
            LastModifiedAt = e.PublishedAt
        };

    public static ManifestReadModel Apply(ManifestReadModel state, ManifestRetiredEventSchema e) =>
        state with { ManifestId = e.AggregateId, Status = "Retired", LastModifiedAt = e.RetiredAt };

    public static ManifestReadModel Apply(ManifestReadModel state, ManifestArchivedEventSchema e) =>
        state with { ManifestId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
