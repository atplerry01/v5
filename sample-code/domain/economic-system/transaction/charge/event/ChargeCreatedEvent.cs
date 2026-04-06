namespace Whycespace.Domain.EconomicSystem.Transaction.Charge;

public sealed record ChargeCreatedEvent(
    Guid ChargeId,
    Guid TargetEntityId,
    decimal Amount,
    string ChargeType) : DomainEvent;
