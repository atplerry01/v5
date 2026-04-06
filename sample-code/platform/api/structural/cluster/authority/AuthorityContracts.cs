namespace Whycespace.Platform.Api.Structural.Cluster.Authority;

public sealed record AuthorityRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AuthorityResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
