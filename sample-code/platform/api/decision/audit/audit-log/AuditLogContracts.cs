namespace Whycespace.Platform.Api.Decision.Audit.AuditLog;

public sealed record AuditLogRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AuditLogResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
