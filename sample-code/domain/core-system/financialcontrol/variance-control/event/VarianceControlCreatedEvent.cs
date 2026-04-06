using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.VarianceControl;

public sealed record VarianceControlCreatedEvent(Guid ControlId, string ControlName, decimal TolerancePercent) : DomainEvent;
public sealed record VarianceControlToleranceUpdatedEvent(Guid ControlId, decimal NewTolerancePercent) : DomainEvent;
