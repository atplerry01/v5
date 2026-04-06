using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementCreatedEvent(Guid SettlementId) : DomainEvent;
