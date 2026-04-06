namespace Whycespace.Platform.Api.Business.Logistic.Fulfillment;

public sealed record FulfillmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record FulfillmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
