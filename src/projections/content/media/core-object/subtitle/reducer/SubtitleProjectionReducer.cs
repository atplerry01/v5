using Whycespace.Shared.Contracts.Content.Media.CoreObject.Subtitle;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Subtitle;

namespace Whycespace.Projections.Content.Media.CoreObject.Subtitle.Reducer;

public static class SubtitleProjectionReducer
{
    public static SubtitleReadModel Apply(SubtitleReadModel state, SubtitleCreatedEventSchema e) =>
        state with
        {
            SubtitleId = e.AggregateId,
            AssetRef = e.AssetRef,
            SourceFileRef = e.SourceFileRef,
            Format = e.Format,
            Language = e.Language,
            WindowStartMs = e.WindowStartMs,
            WindowEndMs = e.WindowEndMs,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static SubtitleReadModel Apply(SubtitleReadModel state, SubtitleUpdatedEventSchema e) =>
        state with { SubtitleId = e.AggregateId, OutputRef = e.OutputRef, Status = "Active", LastModifiedAt = e.UpdatedAt };

    public static SubtitleReadModel Apply(SubtitleReadModel state, SubtitleFinalizedEventSchema e) =>
        state with { SubtitleId = e.AggregateId, Status = "Finalized", FinalizedAt = e.FinalizedAt, LastModifiedAt = e.FinalizedAt };

    public static SubtitleReadModel Apply(SubtitleReadModel state, SubtitleArchivedEventSchema e) =>
        state with { SubtitleId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
