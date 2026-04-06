namespace Whycespace.Platform.Api.Decision.Audit.Remediation;

public sealed record RemediationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RemediationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
