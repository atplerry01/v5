namespace Whycespace.Platform.Api.Decision.Risk.Rating;

public sealed record RatingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record RatingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
