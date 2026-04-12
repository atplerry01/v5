namespace Whycespace.Domain.BusinessSystem.Subscription.SubscriptionAccount;

public static class SubscriptionAccountErrors
{
    public static InvalidOperationException MissingId()
        => new("Subscription account ID must not be empty.");

    public static InvalidOperationException MissingAccountHolder()
        => new("Account holder must have a non-empty holder reference and holder name.");

    public static InvalidOperationException InvalidStateTransition(SubscriptionAccountStatus status, string action)
        => new($"Cannot perform '{action}' when subscription account status is '{status}'.");
}
