using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record AccountDebitedEvent(Guid AccountId, decimal Amount) : DomainEvent;
