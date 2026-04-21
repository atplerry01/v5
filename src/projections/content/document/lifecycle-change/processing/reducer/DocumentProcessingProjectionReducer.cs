using Whycespace.Shared.Contracts.Content.Document.LifecycleChange.Processing;
using Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Processing;

namespace Whycespace.Projections.Content.Document.LifecycleChange.Processing.Reducer;

public static class DocumentProcessingProjectionReducer
{
    public static DocumentProcessingReadModel Apply(DocumentProcessingReadModel state, DocumentProcessingRequestedEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            Kind = e.Kind,
            InputRef = e.InputRef,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastModifiedAt = e.RequestedAt
        };

    public static DocumentProcessingReadModel Apply(DocumentProcessingReadModel state, DocumentProcessingStartedEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            Status = "Running",
            StartedAt = e.StartedAt,
            LastModifiedAt = e.StartedAt
        };

    public static DocumentProcessingReadModel Apply(DocumentProcessingReadModel state, DocumentProcessingCompletedEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            OutputRef = e.OutputRef,
            Status = "Completed",
            CompletedAt = e.CompletedAt,
            LastModifiedAt = e.CompletedAt
        };

    public static DocumentProcessingReadModel Apply(DocumentProcessingReadModel state, DocumentProcessingFailedEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            Status = "Failed",
            FailureReason = e.Reason,
            CompletedAt = e.FailedAt,
            LastModifiedAt = e.FailedAt
        };

    public static DocumentProcessingReadModel Apply(DocumentProcessingReadModel state, DocumentProcessingCancelledEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            Status = "Cancelled",
            CompletedAt = e.CancelledAt,
            LastModifiedAt = e.CancelledAt
        };
}
