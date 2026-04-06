namespace Whycespace.Engines.T2E.Business.Entitlement.Limit;

public class LimitEngine
{
    private readonly LimitPolicyAdapter _policy;

    public LimitEngine(LimitPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<LimitResult> ExecuteAsync(LimitCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new LimitResult(true, "Executed");
    }
}
