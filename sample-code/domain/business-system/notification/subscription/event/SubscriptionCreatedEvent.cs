using Whycespace.Domain.SharedKernel;

namespace Whycespace.Domain.BusinessSystem.Notification.Subscription;

public sealed record SubscriptionCreatedEvent(
    Guid SubscriptionId,
    Guid IdentityId,
    string EventType
) : DomainEvent;
