using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public readonly record struct VaultId
{
    public Guid Value { get; }

    public VaultId(Guid value)
    {
        Guard.Against(value == Guid.Empty, "VaultId cannot be empty.");
        Value = value;
    }

    public static VaultId From(Guid value) => new(value);
}
