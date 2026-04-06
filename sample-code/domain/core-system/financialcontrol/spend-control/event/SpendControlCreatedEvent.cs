using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.SpendControl;

public sealed record SpendControlCreatedEvent(Guid ControlId, string ControlName, decimal SpendLimit) : DomainEvent;
public sealed record SpendControlSuspendedEvent(Guid ControlId) : DomainEvent;
