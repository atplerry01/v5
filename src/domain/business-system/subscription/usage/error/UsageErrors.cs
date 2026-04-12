namespace Whycespace.Domain.BusinessSystem.Subscription.Usage;

public static class UsageErrors
{
    public static InvalidOperationException MissingId()
        => new("UsageId is required and must not be empty.");

    public static InvalidOperationException MissingRecord()
        => new("UsageRecord is required.");

    public static InvalidOperationException InvalidStateTransition(UsageStatus currentStatus, string attemptedAction)
        => new($"Cannot '{attemptedAction}' when current status is '{currentStatus}'.");
}
