namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeWaivedEvent(Guid ChargeId, string Reason) : DomainEvent;
