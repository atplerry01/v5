namespace Whycespace.Domain.BusinessSystem.Resource.Reservation;

public sealed record ReservationStatus
{
    public string Value { get; }

    private ReservationStatus(string value) => Value = value;

    public static readonly ReservationStatus Active = new("Active");
    public static readonly ReservationStatus Expired = new("Expired");
    public static readonly ReservationStatus Cancelled = new("Cancelled");

    public override string ToString() => Value;
}
