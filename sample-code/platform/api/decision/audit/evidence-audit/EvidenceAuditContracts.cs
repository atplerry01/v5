namespace Whycespace.Platform.Api.Decision.Audit.EvidenceAudit;

public sealed record EvidenceAuditRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record EvidenceAuditResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
