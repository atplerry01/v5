using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public readonly record struct AmendmentId
{
    public Guid Value { get; }

    public AmendmentId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "AmendmentId cannot be empty.");
        Value = value;
    }
}
