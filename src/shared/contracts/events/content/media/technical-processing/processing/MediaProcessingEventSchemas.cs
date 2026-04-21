namespace Whycespace.Shared.Contracts.Events.Content.Media.TechnicalProcessing.Processing;

public sealed record MediaProcessingRequestedEventSchema(
    Guid AggregateId,
    string Kind,
    Guid InputRef,
    DateTimeOffset RequestedAt);

public sealed record MediaProcessingStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record MediaProcessingCompletedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset CompletedAt);

public sealed record MediaProcessingFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record MediaProcessingCancelledEventSchema(
    Guid AggregateId,
    DateTimeOffset CancelledAt);
