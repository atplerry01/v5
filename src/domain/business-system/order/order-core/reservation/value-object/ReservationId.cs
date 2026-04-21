using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct ReservationId
{
    public Guid Value { get; }

    public ReservationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReservationId cannot be empty.");
        Value = value;
    }
}
