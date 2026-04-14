using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public sealed record SettlementCompletedEvent(
    string SettlementId,
    string ExternalReferenceId) : DomainEvent;
