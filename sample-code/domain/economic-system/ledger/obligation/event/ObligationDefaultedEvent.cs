using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.EconomicSystem.Ledger.Obligation;

public sealed record ObligationDefaultedEvent(Guid ObligationId, string Reason) : DomainEvent;
