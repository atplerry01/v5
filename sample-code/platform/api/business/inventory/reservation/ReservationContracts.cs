namespace Whycespace.Platform.Api.Business.Inventory.Reservation;

public sealed record ReservationRequest
{
    public string? EntityId { get; init; }
    public object? Payload { get; init; }
}

public sealed record ReservationResponse
{
    public required string EntityId { get; init; }
    public required string Status { get; init; }
}
