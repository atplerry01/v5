using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record VaultSliceCreatedEvent(
    VaultId VaultId,
    SliceId SliceId,
    Amount TotalCapacity,
    Currency Currency) : DomainEvent;
