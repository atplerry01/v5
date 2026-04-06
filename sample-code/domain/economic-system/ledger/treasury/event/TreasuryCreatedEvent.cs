using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Treasury;

public sealed record TreasuryCreatedEvent(Guid TreasuryId) : DomainEvent;
