using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalFundedEvent(
    AccountId AccountId,
    decimal FundedAmount,
    string CurrencyCode,
    decimal NewTotalBalance,
    decimal NewAvailableBalance,
    DateTimeOffset FundedAt) : DomainEvent;
