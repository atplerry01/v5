using Whycespace.Shared.Contracts.Content.Media.LifecycleChange.Version;
using Whycespace.Shared.Contracts.Events.Content.Media.LifecycleChange.Version;

namespace Whycespace.Projections.Content.Media.LifecycleChange.Version.Reducer;

public static class MediaVersionProjectionReducer
{
    public static MediaVersionReadModel Apply(MediaVersionReadModel state, MediaVersionCreatedEventSchema e) =>
        state with
        {
            VersionId = e.AggregateId,
            AssetRef = e.AssetRef,
            VersionMajor = e.VersionMajor,
            VersionMinor = e.VersionMinor,
            FileRef = e.FileRef,
            PreviousVersionId = e.PreviousVersionId,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static MediaVersionReadModel Apply(MediaVersionReadModel state, MediaVersionActivatedEventSchema e) =>
        state with { VersionId = e.AggregateId, Status = "Active", LastModifiedAt = e.ActivatedAt };

    public static MediaVersionReadModel Apply(MediaVersionReadModel state, MediaVersionSupersededEventSchema e) =>
        state with { VersionId = e.AggregateId, Status = "Superseded", SuccessorVersionId = e.SuccessorVersionId, LastModifiedAt = e.SupersededAt };

    public static MediaVersionReadModel Apply(MediaVersionReadModel state, MediaVersionWithdrawnEventSchema e) =>
        state with { VersionId = e.AggregateId, Status = "Withdrawn", LastModifiedAt = e.WithdrawnAt };
}
