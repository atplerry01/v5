using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationActivatedEvent(Guid ObligationId) : DomainEvent;
