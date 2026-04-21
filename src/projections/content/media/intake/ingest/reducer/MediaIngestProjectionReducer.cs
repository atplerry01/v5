using Whycespace.Shared.Contracts.Content.Media.Intake.Ingest;
using Whycespace.Shared.Contracts.Events.Content.Media.Intake.Ingest;

namespace Whycespace.Projections.Content.Media.Intake.Ingest.Reducer;

public static class MediaIngestProjectionReducer
{
    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestRequestedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            SourceRef = e.SourceRef,
            InputRef = e.InputRef,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastModifiedAt = e.RequestedAt
        };

    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestAcceptedEventSchema e) =>
        state with { UploadId = e.AggregateId, Status = "Accepted", AcceptedAt = e.AcceptedAt, LastModifiedAt = e.AcceptedAt };

    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestProcessingStartedEventSchema e) =>
        state with { UploadId = e.AggregateId, Status = "Processing", StartedAt = e.StartedAt, LastModifiedAt = e.StartedAt };

    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestCompletedEventSchema e) =>
        state with { UploadId = e.AggregateId, Status = "Completed", OutputRef = e.OutputRef, CompletedAt = e.CompletedAt, LastModifiedAt = e.CompletedAt };

    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestFailedEventSchema e) =>
        state with { UploadId = e.AggregateId, Status = "Failed", FailureReason = e.Reason, CompletedAt = e.FailedAt, LastModifiedAt = e.FailedAt };

    public static MediaIngestReadModel Apply(MediaIngestReadModel state, MediaIngestCancelledEventSchema e) =>
        state with { UploadId = e.AggregateId, Status = "Cancelled", CompletedAt = e.CancelledAt, LastModifiedAt = e.CancelledAt };
}
