using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct ReservationQuantity
{
    public decimal Value { get; }
    public string Unit { get; }

    public ReservationQuantity(decimal value, string unit)
    {
        Guard.Against(value <= 0m, "ReservationQuantity must be positive.");
        Guard.Against(string.IsNullOrWhiteSpace(unit), "ReservationQuantity unit must not be empty.");

        Value = value;
        Unit = unit!.Trim();
    }
}
