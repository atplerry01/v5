using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public sealed record CapitalAllocatedToSliceEvent(
    string VaultId,
    decimal Amount,
    string FromSlice,
    string ToSlice) : DomainEvent;
