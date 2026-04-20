namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentAcceptedEvent(AmendmentId AmendmentId, DateTimeOffset AcceptedAt);
