using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationFulfilledEvent(
    ObligationId ObligationId,
    Guid SettlementId,
    Timestamp FulfilledAt) : DomainEvent;
