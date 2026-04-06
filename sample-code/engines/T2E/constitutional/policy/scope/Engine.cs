namespace Whycespace.Engines.T2E.Constitutional.Policy.Scope;

public class PolicyScopeEngine
{
    private readonly PolicyScopePolicyAdapter _policy;

    public PolicyScopeEngine(PolicyScopePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PolicyScopeResult> ExecuteAsync(PolicyScopeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new PolicyScopeResult(true, "Executed");
    }
}
