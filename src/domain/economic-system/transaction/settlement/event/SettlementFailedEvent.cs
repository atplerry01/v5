using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public sealed record SettlementFailedEvent(
    string SettlementId,
    string Reason) : DomainEvent;
