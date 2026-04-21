namespace Whycespace.Shared.Contracts.Events.Content.Document.LifecycleChange.Processing;

public sealed record DocumentProcessingRequestedEventSchema(
    Guid AggregateId,
    string Kind,
    Guid InputRef,
    DateTimeOffset RequestedAt);

public sealed record DocumentProcessingStartedEventSchema(
    Guid AggregateId,
    DateTimeOffset StartedAt);

public sealed record DocumentProcessingCompletedEventSchema(
    Guid AggregateId,
    Guid OutputRef,
    DateTimeOffset CompletedAt);

public sealed record DocumentProcessingFailedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);

public sealed record DocumentProcessingCancelledEventSchema(
    Guid AggregateId,
    DateTimeOffset CancelledAt);
