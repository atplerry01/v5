namespace Whycespace.Platform.Api.Business.Integration.Delivery;

public sealed record DeliveryRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record DeliveryResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
