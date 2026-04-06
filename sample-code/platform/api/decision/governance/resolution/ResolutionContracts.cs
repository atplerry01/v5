namespace Whycespace.Platform.Api.Decision.Governance.Resolution;

public sealed record ResolutionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ResolutionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
