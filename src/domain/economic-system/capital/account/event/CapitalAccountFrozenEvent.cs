using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalAccountFrozenEvent(
    AccountId AccountId,
    string Reason,
    decimal TotalBalanceAtFreeze,
    decimal AvailableBalanceAtFreeze,
    decimal ReservedBalanceAtFreeze,
    DateTimeOffset FrozenAt) : DomainEvent;
