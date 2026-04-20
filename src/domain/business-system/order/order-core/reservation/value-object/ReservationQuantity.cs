namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct ReservationQuantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public ReservationQuantity(decimal value, string unit)
    {
        if (value <= 0m)
            throw new ArgumentException("ReservationQuantity must be positive.", nameof(value));

        if (string.IsNullOrWhiteSpace(unit))
            throw new ArgumentException("ReservationQuantity unit must not be empty.", nameof(unit));

        Value = value;
        Unit = unit.Trim();
    }
}
