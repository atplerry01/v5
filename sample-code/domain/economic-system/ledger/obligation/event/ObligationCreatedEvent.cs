using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationCreatedEvent(
    Guid ObligationId,
    Guid DebtorId,
    Guid CreditorId,
    decimal Amount,
    string CurrencyCode) : DomainEvent;
