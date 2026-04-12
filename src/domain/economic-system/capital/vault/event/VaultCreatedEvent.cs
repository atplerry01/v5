using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultCreatedEvent(
    VaultId VaultId,
    Guid OwnerId,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
