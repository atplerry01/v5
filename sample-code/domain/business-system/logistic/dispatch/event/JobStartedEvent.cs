using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobStartedEvent(Guid JobId) : DomainEvent;
