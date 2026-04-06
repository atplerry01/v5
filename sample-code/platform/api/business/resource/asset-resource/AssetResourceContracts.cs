namespace Whycespace.Platform.Api.Business.Resource.AssetResource;

public sealed record AssetResourceRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record AssetResourceResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
