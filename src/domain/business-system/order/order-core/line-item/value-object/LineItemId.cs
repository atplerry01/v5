using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public readonly record struct LineItemId
{
    public Guid Value { get; }

    public LineItemId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LineItemId cannot be empty.");
        Value = value;
    }
}
