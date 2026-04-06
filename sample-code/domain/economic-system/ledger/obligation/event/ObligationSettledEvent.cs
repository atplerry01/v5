using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationSettledEvent(Guid ObligationId) : DomainEvent;
