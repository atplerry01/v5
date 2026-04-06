namespace Whycespace.Platform.Api.Business.Marketplace.Offer;

public sealed record OfferRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record OfferResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
