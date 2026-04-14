using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public sealed record VaultCreditedEvent(
    string VaultId,
    decimal Amount,
    SliceType Slice) : DomainEvent;
