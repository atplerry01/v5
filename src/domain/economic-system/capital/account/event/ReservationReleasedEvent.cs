using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record ReservationReleasedEvent(
    AccountId AccountId,
    decimal ReleasedAmount,
    string CurrencyCode,
    decimal NewAvailableBalance,
    decimal NewReservedBalance,
    DateTimeOffset ReleasedAt) : DomainEvent;
