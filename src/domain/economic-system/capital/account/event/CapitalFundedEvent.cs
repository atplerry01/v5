using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalFundedEvent(
    AccountId AccountId,
    Amount FundedAmount,
    Amount NewTotalBalance,
    Amount NewAvailableBalance) : DomainEvent;
