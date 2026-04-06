using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultLockedEvent(Guid VaultId) : DomainEvent;
