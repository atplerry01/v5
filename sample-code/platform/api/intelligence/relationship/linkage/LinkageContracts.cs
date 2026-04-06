namespace Whycespace.Platform.Api.Intelligence.Relationship.Linkage;

public sealed record LinkageRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record LinkageResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
