namespace Whycespace.Platform.Api.Business.Logistic.Shipment;

public sealed record ShipmentRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ShipmentResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
