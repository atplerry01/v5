namespace Whycespace.Domain.BusinessSystem.Subscription.SubscriptionAccount;

public static class CanActivateSpecification
{
    public static bool IsSatisfiedBy(SubscriptionAccountStatus status) => status == SubscriptionAccountStatus.Created;
}

public static class CanSuspendSpecification
{
    public static bool IsSatisfiedBy(SubscriptionAccountStatus status) => status == SubscriptionAccountStatus.Active;
}

public static class CanReactivateSpecification
{
    public static bool IsSatisfiedBy(SubscriptionAccountStatus status) => status == SubscriptionAccountStatus.Suspended;
}

public static class CanCloseSpecification
{
    public static bool IsSatisfiedBy(SubscriptionAccountStatus status) =>
        status == SubscriptionAccountStatus.Active || status == SubscriptionAccountStatus.Suspended;
}
