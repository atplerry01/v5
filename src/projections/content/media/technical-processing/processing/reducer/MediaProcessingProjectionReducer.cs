using Whycespace.Shared.Contracts.Content.Media.TechnicalProcessing.Processing;
using Whycespace.Shared.Contracts.Events.Content.Media.TechnicalProcessing.Processing;

namespace Whycespace.Projections.Content.Media.TechnicalProcessing.Processing.Reducer;

public static class MediaProcessingProjectionReducer
{
    public static MediaProcessingReadModel Apply(MediaProcessingReadModel state, MediaProcessingRequestedEventSchema e) =>
        state with
        {
            JobId = e.AggregateId,
            Kind = e.Kind,
            InputRef = e.InputRef,
            Status = "Requested",
            RequestedAt = e.RequestedAt,
            LastModifiedAt = e.RequestedAt
        };

    public static MediaProcessingReadModel Apply(MediaProcessingReadModel state, MediaProcessingStartedEventSchema e) =>
        state with { JobId = e.AggregateId, Status = "Running", StartedAt = e.StartedAt, LastModifiedAt = e.StartedAt };

    public static MediaProcessingReadModel Apply(MediaProcessingReadModel state, MediaProcessingCompletedEventSchema e) =>
        state with { JobId = e.AggregateId, Status = "Completed", OutputRef = e.OutputRef, CompletedAt = e.CompletedAt, LastModifiedAt = e.CompletedAt };

    public static MediaProcessingReadModel Apply(MediaProcessingReadModel state, MediaProcessingFailedEventSchema e) =>
        state with { JobId = e.AggregateId, Status = "Failed", FailureReason = e.Reason, CompletedAt = e.FailedAt, LastModifiedAt = e.FailedAt };

    public static MediaProcessingReadModel Apply(MediaProcessingReadModel state, MediaProcessingCancelledEventSchema e) =>
        state with { JobId = e.AggregateId, Status = "Cancelled", CompletedAt = e.CancelledAt, LastModifiedAt = e.CancelledAt };
}
