namespace Whycespace.Domain.BusinessSystem.Execution.Charge;

public sealed record ChargeCreatedEvent(ChargeId ChargeId, CostSourceId CostSourceId);
