using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalAccountOpenedEvent(
    AccountId AccountId,
    OwnerId OwnerId,
    string CurrencyCode,
    DateTimeOffset OpenedAt) : DomainEvent;
