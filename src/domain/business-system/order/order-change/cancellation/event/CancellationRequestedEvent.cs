using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed record CancellationRequestedEvent(
    CancellationId CancellationId,
    OrderRef Order,
    CancellationReason Reason,
    DateTimeOffset RequestedAt);
