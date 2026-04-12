using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementCompletedEvent(
    SettlementId SettlementId,
    Timestamp CompletedAt) : DomainEvent;
