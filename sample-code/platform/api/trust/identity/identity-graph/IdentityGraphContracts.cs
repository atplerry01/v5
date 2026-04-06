namespace Whycespace.Platform.Api.Trust.Identity.IdentityGraph;

public sealed record IdentityGraphRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record IdentityGraphResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
