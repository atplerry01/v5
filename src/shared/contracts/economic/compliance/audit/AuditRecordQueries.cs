namespace Whycespace.Shared.Contracts.Economic.Compliance.Audit;

public sealed record GetAuditRecordByIdQuery(Guid AuditRecordId);

public sealed record ListAuditRecordsBySourceQuery(
    string SourceDomain,
    Guid SourceAggregateId);
