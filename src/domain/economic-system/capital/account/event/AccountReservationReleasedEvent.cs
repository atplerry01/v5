using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record AccountReservationReleasedEvent(
    AccountId AccountId,
    Amount ReleasedAmount,
    Amount NewAvailableBalance,
    Amount NewReservedBalance) : DomainEvent;
