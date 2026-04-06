namespace Whycespace.Platform.Api.Intelligence.Relationship.Influence;

public sealed record InfluenceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record InfluenceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
