namespace Whycespace.Platform.Api.Business.Execution.Cost;

public sealed record CostRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
