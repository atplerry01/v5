using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Vault;

public sealed record CapitalAllocatedToSliceEvent(
    VaultId VaultId,
    SliceId SliceId,
    Amount AllocatedAmount,
    Amount NewSliceAvailable,
    Amount NewSliceUsed) : DomainEvent;
