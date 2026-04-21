using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public readonly record struct CancellationId
{
    public Guid Value { get; }

    public CancellationId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "CancellationId cannot be empty.");
        Value = value;
    }
}
