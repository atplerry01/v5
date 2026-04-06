namespace Whycespace.Platform.Api.Intelligence.Index.PriceIndex;

public sealed record PriceIndexRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PriceIndexResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
