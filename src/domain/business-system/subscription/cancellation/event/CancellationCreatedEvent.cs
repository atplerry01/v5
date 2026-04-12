namespace Whycespace.Domain.BusinessSystem.Subscription.Cancellation;

public sealed record CancellationRequestedEvent(CancellationId CancellationId, CancellationRequest Request);

public sealed record CancellationConfirmedEvent(CancellationId CancellationId);
