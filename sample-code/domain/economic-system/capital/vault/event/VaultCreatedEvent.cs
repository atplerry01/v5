using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultCreatedEvent(Guid VaultId) : DomainEvent;
