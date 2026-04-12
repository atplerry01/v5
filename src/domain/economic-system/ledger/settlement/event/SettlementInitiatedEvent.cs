using Whycespace.Domain.SharedKernel.Primitive.Money;
using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Settlement;

public sealed record SettlementInitiatedEvent(
    SettlementId SettlementId,
    Guid JournalId,
    Guid ObligationId,
    Amount Amount,
    Currency Currency,
    Timestamp InitiatedAt) : DomainEvent;
