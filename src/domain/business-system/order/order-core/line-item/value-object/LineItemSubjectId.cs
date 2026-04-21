using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderCore.LineItem;

public readonly record struct LineItemSubjectId
{
    public Guid Value { get; }

    public LineItemSubjectId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "LineItemSubjectId cannot be empty.");
        Value = value;
    }
}
