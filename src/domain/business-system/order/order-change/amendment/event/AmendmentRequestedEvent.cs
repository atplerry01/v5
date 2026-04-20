using Whycespace.Domain.BusinessSystem.Shared.Reference;

namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentRequestedEvent(
    AmendmentId AmendmentId,
    OrderRef Order,
    AmendmentReason Reason,
    DateTimeOffset RequestedAt);
