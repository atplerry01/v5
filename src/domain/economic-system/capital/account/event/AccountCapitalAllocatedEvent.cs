using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record AccountCapitalAllocatedEvent(
    AccountId AccountId,
    Amount AllocatedAmount,
    Amount NewAvailableBalance) : DomainEvent;
