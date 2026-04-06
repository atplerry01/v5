namespace Whycespace.Platform.Api.Intelligence.Relationship.Graph;

public sealed record GraphRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record GraphResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
