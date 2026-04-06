namespace Whycespace.Engines.T2E.Business.Subscription.Cancellation;

public record CancellationCommand(
    string Action,
    string EntityId,
    object Payload
);
