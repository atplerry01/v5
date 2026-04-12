namespace Whycespace.Domain.BusinessSystem.Subscription.Plan;

public static class PlanErrors
{
    public static InvalidOperationException MissingId() =>
        new("PlanId is required and must not be empty.");

    public static InvalidOperationException MissingDescriptor() =>
        new("Plan must include a valid descriptor.");

    public static InvalidOperationException InvalidStateTransition(PlanStatus current, string attemptedAction) =>
        new($"Cannot '{attemptedAction}' when current status is '{current}'.");
}
