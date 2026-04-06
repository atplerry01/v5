namespace Whycespace.Platform.Api.Intelligence.Cost.CostDriver;

public sealed record CostDriverRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostDriverResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
