using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobAssignedEvent(Guid JobId, Guid OperatorId) : DomainEvent;
