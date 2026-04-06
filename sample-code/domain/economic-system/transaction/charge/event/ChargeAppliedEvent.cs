namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeAppliedEvent(
    Guid ChargeId,
    Guid TargetEntityId,
    decimal Amount) : DomainEvent;
