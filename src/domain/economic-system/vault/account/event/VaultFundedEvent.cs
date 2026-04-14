using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public sealed record VaultFundedEvent(
    string VaultId,
    decimal Amount,
    string Currency) : DomainEvent;
