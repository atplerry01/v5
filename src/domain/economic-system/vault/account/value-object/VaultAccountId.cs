namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public readonly record struct VaultAccountId
{
    public Guid Value { get; }

    public VaultAccountId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("VaultAccountId cannot be empty.", nameof(value));

        Value = value;
    }

    public static VaultAccountId From(Guid value) => new(value);
}
