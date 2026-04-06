namespace Whycespace.Platform.Api.Business.Marketplace.Bid;

public sealed record BidRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record BidResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
