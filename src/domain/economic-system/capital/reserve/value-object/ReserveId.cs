using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Reserve;

public readonly record struct ReserveId
{
    public Guid Value { get; }

    public ReserveId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "ReserveId cannot be empty.");
        Value = value;
    }

    public static ReserveId From(Guid value) => new(value);
}
