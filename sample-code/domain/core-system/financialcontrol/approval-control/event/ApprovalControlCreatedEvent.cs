using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.ApprovalControl;

public sealed record ApprovalControlCreatedEvent(Guid ControlId, string ControlName, decimal ThresholdAmount) : DomainEvent;
public sealed record ApprovalControlDeactivatedEvent(Guid ControlId) : DomainEvent;
