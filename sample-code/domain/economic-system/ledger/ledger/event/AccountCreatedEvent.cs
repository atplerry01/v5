using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Ledger;

public sealed record AccountCreatedEvent(Guid AccountId) : DomainEvent;
