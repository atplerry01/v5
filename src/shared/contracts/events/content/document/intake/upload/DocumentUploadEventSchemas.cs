namespace Whycespace.Shared.Contracts.Events.Content.Document.Intake.Upload;

public sealed record DocumentUploadRequestedEventSchema(
    Guid AggregateId,
    Guid SourceRef,
    Guid InputRef,
    DateTimeOffset RequestedAt);

public sealed record DocumentUploadAcceptedEventSchema(
    Guid AggregateId,
    DateTimeOffset AcceptedAt);

public sealed record DocumentUploadProcessingStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record DocumentUploadCompletedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset CompletedAt);

public sealed record DocumentUploadFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record DocumentUploadCancelledEventSchema(
    Guid AggregateId,
    DateTimeOffset CancelledAt);
