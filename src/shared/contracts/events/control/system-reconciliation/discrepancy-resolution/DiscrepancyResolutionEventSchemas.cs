namespace Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyResolution;

public sealed record DiscrepancyResolutionInitiatedEventSchema(
    Guid AggregateId,
    string DetectionId,
    DateTimeOffset InitiatedAt);

public sealed record DiscrepancyResolutionCompletedEventSchema(
    Guid AggregateId,
    string Outcome,
    string Notes,
    DateTimeOffset CompletedAt);
