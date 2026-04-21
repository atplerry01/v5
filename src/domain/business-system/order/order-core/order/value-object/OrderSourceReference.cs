using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.Order;

public readonly record struct OrderSourceReference
{
    public Guid Value { get; }

    public OrderSourceReference(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OrderSourceReference cannot be empty.");
        Value = value;
    }
}
