using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementFailedEvent(
    SettlementId SettlementId,
    string Reason,
    Timestamp FailedAt) : DomainEvent;
