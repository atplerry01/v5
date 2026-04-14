using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public sealed record SettlementProcessingStartedEvent(
    string SettlementId) : DomainEvent;
