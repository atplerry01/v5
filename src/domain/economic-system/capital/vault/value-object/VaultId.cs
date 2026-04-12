namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public readonly record struct VaultId
{
    public Guid Value { get; }

    public VaultId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("VaultId cannot be empty.", nameof(value));

        Value = value;
    }

    public static VaultId From(Guid value) => new(value);
}
