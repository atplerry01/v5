namespace Whycespace.Shared.Contracts.Events.Economic.Enforcement.Restriction;

public sealed record RestrictionAppliedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string Scope,
    string Reason,
    DateTimeOffset AppliedAt);

public sealed record RestrictionUpdatedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string NewScope,
    string NewReason,
    DateTimeOffset UpdatedAt);

public sealed record RestrictionRemovedEventSchema(
    Guid AggregateId,
    Guid SubjectId,
    string RemovalReason,
    DateTimeOffset RemovedAt);
