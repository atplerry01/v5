namespace Whycespace.Platform.Api.Core.Temporal.TimeWindow;

public sealed record TimeWindowRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TimeWindowResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
