namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeApprovedEvent(Guid ChargeId) : DomainEvent;
