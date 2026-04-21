using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Reservation;

public readonly record struct LineItemRef
{
    public Guid Value { get; }

    public LineItemRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LineItemRef cannot be empty.");
        Value = value;
    }
}
