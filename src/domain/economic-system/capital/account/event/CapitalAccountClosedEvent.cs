using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Capital.Account;

public sealed record CapitalAccountClosedEvent(
    AccountId AccountId,
    string Reason,
    DateTimeOffset ClosedAt) : DomainEvent;
