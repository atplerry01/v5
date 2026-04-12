using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public readonly record struct OwnerId
{
    public Guid Value { get; }

    public OwnerId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "OwnerId cannot be empty.");
        Value = value;
    }
}
