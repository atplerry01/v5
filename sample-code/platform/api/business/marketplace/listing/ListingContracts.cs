namespace Whycespace.Platform.Api.Business.Marketplace.Listing;

public sealed record ListingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ListingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
