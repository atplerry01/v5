namespace Whycespace.Engines.T2E.Business.Notification.Subscription;

public class SubscriptionEngine
{
    private readonly SubscriptionPolicyAdapter _policy;

    public SubscriptionEngine(SubscriptionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SubscriptionResult> ExecuteAsync(SubscriptionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SubscriptionResult(true, "Executed");
    }
}
