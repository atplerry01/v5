namespace Whycespace.Platform.Api.Decision.Audit.Finding;

public sealed record FindingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record FindingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
