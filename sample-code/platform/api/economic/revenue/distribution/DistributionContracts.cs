namespace Whycespace.Platform.Api.Economic.Revenue.Distribution;

public sealed record DistributionRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DistributionResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
