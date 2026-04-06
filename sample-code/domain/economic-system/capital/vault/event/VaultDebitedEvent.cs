using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultDebitedEvent(Guid VaultId, decimal Amount) : DomainEvent;
