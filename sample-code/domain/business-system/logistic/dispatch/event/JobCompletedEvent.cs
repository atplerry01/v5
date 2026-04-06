using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobCompletedEvent(Guid JobId) : DomainEvent;
