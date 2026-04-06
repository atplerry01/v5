namespace Whycespace.Platform.Api.Decision.Compliance.ComplianceCase;

public sealed record ComplianceCaseRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ComplianceCaseResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
