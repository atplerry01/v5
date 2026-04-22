namespace Whycespace.Shared.Contracts.Control.Audit.AuditTrace;

public sealed record AuditTraceReadModel
{
    public Guid TraceId { get; init; }
    public string CorrelationId { get; init; } = string.Empty;
    public DateTimeOffset OpenedAt { get; init; }
    public DateTimeOffset? ClosedAt { get; init; }
    public string Status { get; init; } = string.Empty;
    public IReadOnlyList<string> LinkedEventIds { get; init; } = [];
}
