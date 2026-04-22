namespace Whycespace.Shared.Contracts.Events.Control.SystemReconciliation.SystemVerification;

public sealed record SystemVerificationInitiatedEventSchema(
    Guid AggregateId,
    string TargetSystem,
    DateTimeOffset InitiatedAt);

public sealed record SystemVerificationPassedEventSchema(
    Guid AggregateId,
    DateTimeOffset PassedAt);

public sealed record SystemVerificationFailedEventSchema(
    Guid AggregateId,
    string FailureReason,
    DateTimeOffset FailedAt);
