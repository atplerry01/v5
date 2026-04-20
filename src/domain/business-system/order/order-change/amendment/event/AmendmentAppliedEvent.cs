namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Amendment;

public sealed record AmendmentAppliedEvent(AmendmentId AmendmentId, DateTimeOffset AppliedAt);
