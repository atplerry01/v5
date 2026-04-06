namespace Whycespace.Projections.Business.Inventory.Reservation;

public sealed record ReservationView
{
    public required string Id { get; init; }
    public required string Status { get; init; }
    public DateTimeOffset LastUpdated { get; init; }
}
