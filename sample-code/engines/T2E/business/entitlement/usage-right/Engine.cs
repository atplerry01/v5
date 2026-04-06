namespace Whycespace.Engines.T2E.Business.Entitlement.UsageRight;

public class UsageRightEngine
{
    private readonly UsageRightPolicyAdapter _policy;

    public UsageRightEngine(UsageRightPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<UsageRightResult> ExecuteAsync(UsageRightCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new UsageRightResult(true, "Executed");
    }
}
