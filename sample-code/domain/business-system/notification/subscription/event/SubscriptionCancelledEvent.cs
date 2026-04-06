using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed record SubscriptionCancelledEvent(Guid SubscriptionId) : DomainEvent;
