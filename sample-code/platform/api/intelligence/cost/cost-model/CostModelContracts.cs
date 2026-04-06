namespace Whycespace.Platform.Api.Intelligence.Cost.CostModel;

public sealed record CostModelRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostModelResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
