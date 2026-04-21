using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.FulfillmentInstruction;

public readonly record struct OrderRef
{
    public Guid Value { get; }

    public OrderRef(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OrderRef cannot be empty.");
        Value = value;
    }
}
