namespace Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.DiscrepancyDetection;

public sealed record DiscrepancyDetectedEventSchema(
    Guid AggregateId,
    string Kind,
    string SourceReference,
    DateTimeOffset DetectedAt);

public sealed record DiscrepancyDetectionDismissedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset DismissedAt);
