namespace Whycespace.Platform.Api.Decision.Governance.Review;

public sealed record ReviewRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReviewResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
