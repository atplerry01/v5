namespace Whycespace.Platform.Api.Decision.Risk.Mitigation;

public sealed record MitigationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record MitigationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
