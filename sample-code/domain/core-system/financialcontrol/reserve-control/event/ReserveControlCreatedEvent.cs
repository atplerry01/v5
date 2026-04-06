using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.CoreSystem.FinancialControl.ReserveControl;

public sealed record ReserveControlCreatedEvent(Guid ControlId, string ReserveName, decimal MinimumBalance) : DomainEvent;
public sealed record ReserveControlMinimumUpdatedEvent(Guid ControlId, decimal NewMinimumBalance) : DomainEvent;
