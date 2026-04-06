namespace Whycespace.Platform.Api.Economic.Revenue.Pricing;

public sealed record PricingRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record PricingResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
