namespace Whycespace.Platform.Api.Structural.Humancapital.Incentive;

public sealed record IncentiveRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IncentiveResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
