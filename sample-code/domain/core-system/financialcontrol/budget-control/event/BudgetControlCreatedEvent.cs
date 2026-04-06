using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.BudgetControl;

public sealed record BudgetControlCreatedEvent(Guid ControlId, string BudgetName, decimal AllocatedAmount) : DomainEvent;
public sealed record BudgetControlAdjustedEvent(Guid ControlId, decimal NewAmount) : DomainEvent;
