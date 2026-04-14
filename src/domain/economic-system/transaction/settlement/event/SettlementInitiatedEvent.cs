using Whycespace.Domain.SharedKernel.Primitives.Kernel;

namespace Whycespace.Domain.EconomicSystem.Transaction.Settlement;

public sealed record SettlementInitiatedEvent(
    string SettlementId,
    decimal Amount,
    string Currency,
    string SourceReference,
    string Provider) : DomainEvent;
