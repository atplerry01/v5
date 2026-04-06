using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Logistic.Dispatch;

public sealed record JobCancelledEvent(Guid JobId, string Reason) : DomainEvent;
