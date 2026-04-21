using Whycespace.Shared.Contracts.Content.Document.Intake.Upload;
using Whycespace.Shared.Contracts.Events.Content.Document.Intake.Upload;

namespace Whycespace.Projections.Content.Document.Intake.Upload.Reducer;

public static class DocumentUploadProjectionReducer
{
    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadRequestedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            SourceRef = e.SourceRef,
            InputRef = e.InputRef,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastModifiedAt = e.RequestedAt
        };

    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadAcceptedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            Status = "Accepted",
            AcceptedAt = e.AcceptedAt,
            LastModifiedAt = e.AcceptedAt
        };

    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadProcessingStartedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            Status = "Processing",
            StartedAt = e.StartedAt,
            LastModifiedAt = e.StartedAt
        };

    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadCompletedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            OutputRef = e.OutputRef,
            Status = "Completed",
            CompletedAt = e.CompletedAt,
            LastModifiedAt = e.CompletedAt
        };

    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadFailedEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            Status = "Failed",
            FailureReason = e.Reason,
            CompletedAt = e.FailedAt,
            LastModifiedAt = e.FailedAt
        };

    public static DocumentUploadReadModel Apply(DocumentUploadReadModel state, DocumentUploadCancelledEventSchema e) =>
        state with
        {
            UploadId = e.AggregateId,
            Status = "Cancelled",
            CompletedAt = e.CancelledAt,
            LastModifiedAt = e.CancelledAt
        };
}
