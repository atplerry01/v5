using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationCancelledEvent(
    ObligationId ObligationId,
    string Reason,
    Timestamp CancelledAt) : DomainEvent;
