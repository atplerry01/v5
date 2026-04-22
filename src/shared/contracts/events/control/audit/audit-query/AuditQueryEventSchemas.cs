namespace Whycespace.Shared.Contracts.Events.Control.Audit.AuditQuery;

public sealed record AuditQueryIssuedEventSchema(
    Guid AggregateId,
    string IssuedBy,
    DateTimeOffset TimeRangeFrom,
    DateTimeOffset TimeRangeTo,
    string? CorrelationFilter,
    string? ActorFilter);

public sealed record AuditQueryCompletedEventSchema(
    Guid AggregateId,
    int ResultCount);

public sealed record AuditQueryExpiredEventSchema(
    Guid AggregateId);
