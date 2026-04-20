namespace Whycespace.Domain.BusinessSystem.Order.OrderChange.Cancellation;

public sealed record CancellationConfirmedEvent(CancellationId CancellationId, DateTimeOffset ConfirmedAt);
