using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record CapitalWithdrawnEvent(
    VaultId VaultId,
    SliceId SliceId,
    Amount WithdrawnAmount,
    Amount NewSliceCapacity,
    Amount NewVaultTotal) : DomainEvent;
