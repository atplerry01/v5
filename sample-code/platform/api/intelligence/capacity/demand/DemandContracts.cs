namespace Whycespace.Platform.Api.Intelligence.Capacity.Demand;

public sealed record DemandRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DemandResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
