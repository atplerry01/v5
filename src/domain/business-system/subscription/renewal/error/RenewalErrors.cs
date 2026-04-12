namespace Whycespace.Domain.BusinessSystem.Subscription.Renewal;

public static class RenewalErrors
{
    public static InvalidOperationException MissingId()
        => new("RenewalId is required and must not be empty.");

    public static InvalidOperationException MissingRequest()
        => new("RenewalRequest is required and must not be null.");

    public static InvalidOperationException InvalidStateTransition(RenewalStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
