namespace Whycespace.Shared.Contracts.Events.Content.Media.Intake.Ingest;

public sealed record MediaIngestRequestedEventSchema(
    Guid AggregateId,
    Guid SourceRef,
    Guid InputRef,
    DateTimeOffset RequestedAt);

public sealed record MediaIngestAcceptedEventSchema(
    Guid AggregateId,
    DateTimeOffset AcceptedAt);

public sealed record MediaIngestProcessingStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record MediaIngestCompletedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset CompletedAt);

public sealed record MediaIngestFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record MediaIngestCancelledEventSchema(
    Guid AggregateId,
    DateTimeOffset CancelledAt);
