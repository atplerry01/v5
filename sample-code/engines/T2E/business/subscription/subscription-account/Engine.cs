namespace Whycespace.Engines.T2E.Business.Subscription.SubscriptionAccount;

public class SubscriptionAccountEngine
{
    private readonly SubscriptionAccountPolicyAdapter _policy;

    public SubscriptionAccountEngine(SubscriptionAccountPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SubscriptionAccountResult> ExecuteAsync(SubscriptionAccountCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SubscriptionAccountResult(true, "Executed");
    }
}
