namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed record CancellationRejectedEvent(CancellationId CancellationId, DateTimeOffset RejectedAt);
