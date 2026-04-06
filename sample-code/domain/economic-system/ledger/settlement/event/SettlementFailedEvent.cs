using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementFailedEvent(
    Guid SettlementId,
    string Reason) : DomainEvent;
