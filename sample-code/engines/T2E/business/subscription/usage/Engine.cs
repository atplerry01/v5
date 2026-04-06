namespace Whycespace.Engines.T2E.Business.Subscription.Usage;

public class UsageEngine
{
    private readonly UsagePolicyAdapter _policy;

    public UsageEngine(UsagePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<UsageResult> ExecuteAsync(UsageCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new UsageResult(true, "Executed");
    }
}
