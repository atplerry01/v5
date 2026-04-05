using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalReservedEvent(
    AccountId AccountId,
    decimal ReservedAmount,
    string CurrencyCode,
    decimal NewAvailableBalance,
    decimal NewReservedBalance,
    DateTimeOffset ReservedAt) : DomainEvent;
