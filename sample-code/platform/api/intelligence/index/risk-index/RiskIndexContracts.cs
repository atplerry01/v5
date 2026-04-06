namespace Whycespace.Platform.Api.Intelligence.Index.RiskIndex;

public sealed record RiskIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RiskIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
