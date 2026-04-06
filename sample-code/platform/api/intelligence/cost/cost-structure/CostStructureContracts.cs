namespace Whycespace.Platform.Api.Intelligence.Cost.CostStructure;

public sealed record CostStructureRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record CostStructureResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
