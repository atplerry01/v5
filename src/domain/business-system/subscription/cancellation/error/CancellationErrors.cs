namespace Whycespace.Domain.BusinessSystem.Subscription.Cancellation;

public static class CancellationErrors
{
    public static InvalidOperationException MissingId()
        => new("Cancellation ID must not be empty.");

    public static InvalidOperationException MissingRequest()
        => new("Cancellation request must have a non-empty enrollment reference and reason.");

    public static InvalidOperationException InvalidStateTransition(CancellationStatus status, string action)
        => new($"Cannot perform '{action}' when cancellation status is '{status}'.");
}
