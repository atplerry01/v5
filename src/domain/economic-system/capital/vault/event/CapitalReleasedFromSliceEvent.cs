using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record CapitalReleasedFromSliceEvent(
    VaultId VaultId,
    SliceId SliceId,
    Amount ReleasedAmount,
    Amount NewSliceAvailable,
    Amount NewSliceUsed) : DomainEvent;
