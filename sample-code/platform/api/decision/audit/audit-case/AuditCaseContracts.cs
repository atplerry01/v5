namespace Whycespace.Platform.Api.Decision.Audit.AuditCase;

public sealed record AuditCaseRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AuditCaseResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
