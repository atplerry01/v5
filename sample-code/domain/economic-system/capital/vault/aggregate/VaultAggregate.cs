using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public class VaultAggregate : AggregateRoot
{
    private VaultBalance _balance = new(0m);
    private readonly List<VaultLock> _locks = [];

    public void Create(Guid id)
    {
        Id = id;
        RaiseDomainEvent(new VaultCreatedEvent(id));
    }

    public void Credit(Guid vaultId, decimal amount, string currencyCode)
    {
        EnsureInvariant(amount > 0, "PositiveAmount", "Deposit amount must be positive.");
        _balance = new VaultBalance(_balance.Balance + amount);
        RaiseDomainEvent(new VaultCreditedEvent(vaultId, amount));
    }

    public void Debit(Guid vaultId, decimal amount, string currencyCode)
    {
        EnsureInvariant(amount > 0, "PositiveAmount", "Withdrawal amount must be positive.");
        EnsureInvariant(_balance.Balance >= amount, "SufficientBalance", "Insufficient vault balance.");
        _balance = new VaultBalance(_balance.Balance - amount);
        RaiseDomainEvent(new VaultDebitedEvent(vaultId, amount));
    }

    public void Lock(Guid vaultId, decimal amount, string reason)
    {
        EnsureInvariant(amount > 0, "PositiveAmount", "Lock amount must be positive.");
        EnsureInvariant(_balance.Balance >= amount, "SufficientBalance", "Insufficient balance to lock.");
        RaiseDomainEvent(new VaultLockedEvent(vaultId));
    }
}
