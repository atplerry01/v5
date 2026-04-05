using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalAllocatedEvent(
    AccountId AccountId,
    decimal AllocatedAmount,
    string CurrencyCode,
    decimal NewTotalBalance,
    decimal NewAvailableBalance,
    DateTimeOffset AllocatedAt) : DomainEvent;
