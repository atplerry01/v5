namespace Whycespace.Platform.Api.Economic.Capital.Asset;

public sealed record AssetRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AssetResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
