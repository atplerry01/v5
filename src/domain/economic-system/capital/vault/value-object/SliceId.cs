using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public readonly record struct SliceId
{
    public Guid Value { get; }

    public SliceId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "SliceId cannot be empty.");
        Value = value;
    }

    public static SliceId From(Guid value) => new(value);
}
