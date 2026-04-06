namespace Whycespace.Engines.T2E.Decision.Governance.Scope;

public class ScopeEngine
{
    private readonly ScopePolicyAdapter _policy;

    public ScopeEngine(ScopePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ScopeResult> ExecuteAsync(ScopeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ScopeResult(true, "Executed");
    }
}
