using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultCreditedEvent(Guid VaultId, decimal Amount) : DomainEvent;
