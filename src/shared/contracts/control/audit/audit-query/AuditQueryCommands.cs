using Whycespace.Shared.Contracts.Runtime;

namespace Whycespace.Shared.Contracts.Control.Audit.AuditQuery;

public sealed record IssueAuditQueryCommand(
    Guid QueryId,
    string IssuedBy,
    DateTimeOffset TimeRangeFrom,
    DateTimeOffset TimeRangeTo,
    string? CorrelationFilter = null,
    string? ActorFilter = null) : IHasAggregateId
{
    public Guid AggregateId => QueryId;
}

public sealed record CompleteAuditQueryCommand(
    Guid QueryId,
    int ResultCount) : IHasAggregateId
{
    public Guid AggregateId => QueryId;
}

public sealed record ExpireAuditQueryCommand(
    Guid QueryId) : IHasAggregateId
{
    public Guid AggregateId => QueryId;
}
