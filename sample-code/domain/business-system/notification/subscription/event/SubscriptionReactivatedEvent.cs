using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed record SubscriptionReactivatedEvent(Guid SubscriptionId) : DomainEvent;
