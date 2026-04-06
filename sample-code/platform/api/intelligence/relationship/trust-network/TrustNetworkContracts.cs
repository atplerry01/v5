namespace Whycespace.Platform.Api.Intelligence.Relationship.TrustNetwork;

public sealed record TrustNetworkRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record TrustNetworkResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
