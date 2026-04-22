namespace Whycespace.Shared.Contracts.Control.Audit.AuditQuery;

public sealed record AuditQueryReadModel
{
    public Guid QueryId { get; init; }
    public string IssuedBy { get; init; } = string.Empty;
    public DateTimeOffset TimeRangeFrom { get; init; }
    public DateTimeOffset TimeRangeTo { get; init; }
    public string? CorrelationFilter { get; init; }
    public string? ActorFilter { get; init; }
    public string Status { get; init; } = string.Empty;
    public int? ResultCount { get; init; }
}
