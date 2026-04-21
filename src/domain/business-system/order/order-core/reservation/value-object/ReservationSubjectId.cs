using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct ReservationSubjectId
{
    public Guid Value { get; }

    public ReservationSubjectId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReservationSubjectId cannot be empty.");
        Value = value;
    }
}
