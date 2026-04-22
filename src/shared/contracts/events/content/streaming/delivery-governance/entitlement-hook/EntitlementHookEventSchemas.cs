namespace Whycespace.Shared.Contracts.Events.Content.Streaming.DeliveryGovernance.EntitlementHook;

public sealed record EntitlementHookRegisteredEventSchema(
    Guid AggregateId,
    Guid TargetId,
    string SourceSystem,
    DateTimeOffset RegisteredAt);

public sealed record EntitlementQueriedEventSchema(
    Guid AggregateId,
    string Result,
    DateTimeOffset QueriedAt);

public sealed record EntitlementRefreshedEventSchema(
    Guid AggregateId,
    string Result,
    DateTimeOffset RefreshedAt);

public sealed record EntitlementInvalidatedEventSchema(
    Guid AggregateId,
    DateTimeOffset InvalidatedAt);

public sealed record EntitlementFailureRecordedEventSchema(
    Guid AggregateId,
    string Reason,
    DateTimeOffset FailedAt);
