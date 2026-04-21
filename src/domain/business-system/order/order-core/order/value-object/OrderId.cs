using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public readonly record struct OrderId
{
    public Guid Value { get; }

    public OrderId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OrderId cannot be empty.");
        Value = value;
    }
}
