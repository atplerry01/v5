using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record AccountCapitalReservedEvent(
    AccountId AccountId,
    Amount ReservedAmount,
    Amount NewAvailableBalance,
    Amount NewReservedBalance) : DomainEvent;
