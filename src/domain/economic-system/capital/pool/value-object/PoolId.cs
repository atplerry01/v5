using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Pool;

public readonly record struct PoolId
{
    public Guid Value { get; }

    public PoolId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "PoolId cannot be empty.");
        Value = value;
    }
}
