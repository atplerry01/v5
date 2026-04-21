using Whycespace.Shared.Contracts.Content.Media.CoreObject.Transcript;
using Whycespace.Shared.Contracts.Events.Content.Media.CoreObject.Transcript;

namespace Whycespace.Projections.Content.Media.CoreObject.Transcript.Reducer;

public static class TranscriptProjectionReducer
{
    public static TranscriptReadModel Apply(TranscriptReadModel state, TranscriptCreatedEventSchema e) =>
        state with
        {
            TranscriptId = e.AggregateId,
            AssetRef = e.AssetRef,
            SourceFileRef = e.SourceFileRef,
            Format = e.Format,
            Language = e.Language,
            Status = "Draft",
            CreatedAt = e.CreatedAt,
            LastModifiedAt = e.CreatedAt
        };

    public static TranscriptReadModel Apply(TranscriptReadModel state, TranscriptUpdatedEventSchema e) =>
        state with { TranscriptId = e.AggregateId, OutputRef = e.OutputRef, Status = "Active", LastModifiedAt = e.UpdatedAt };

    public static TranscriptReadModel Apply(TranscriptReadModel state, TranscriptFinalizedEventSchema e) =>
        state with { TranscriptId = e.AggregateId, Status = "Finalized", FinalizedAt = e.FinalizedAt, LastModifiedAt = e.FinalizedAt };

    public static TranscriptReadModel Apply(TranscriptReadModel state, TranscriptArchivedEventSchema e) =>
        state with { TranscriptId = e.AggregateId, Status = "Archived", LastModifiedAt = e.ArchivedAt };
}
