using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record CapitalDepositedEvent(
    VaultId VaultId,
    SliceId SliceId,
    Amount DepositedAmount,
    Amount NewSliceCapacity,
    Amount NewVaultTotal) : DomainEvent;
