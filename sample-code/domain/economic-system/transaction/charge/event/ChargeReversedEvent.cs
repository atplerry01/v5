namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeReversedEvent(Guid ChargeId, string Reason) : DomainEvent;
