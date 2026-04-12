using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationCreatedEvent(
    ObligationId ObligationId,
    Guid CounterpartyId,
    ObligationType Type,
    Amount Amount,
    Currency Currency,
    Timestamp CreatedAt) : DomainEvent;
