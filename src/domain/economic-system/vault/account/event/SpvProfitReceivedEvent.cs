using Whycespace.Domain.EconomicSystem.Vault.Slice;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Vault.Account;

public sealed record SpvProfitReceivedEvent(
    string VaultId,
    decimal Amount,
    string Currency,
    SliceType Slice) : DomainEvent;
