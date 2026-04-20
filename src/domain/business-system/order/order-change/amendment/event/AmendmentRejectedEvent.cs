namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentRejectedEvent(AmendmentId AmendmentId, DateTimeOffset RejectedAt);
