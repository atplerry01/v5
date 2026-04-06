namespace Whycespace.Engines.T2E.Decision.Governance.Sanction;

public class SanctionEngine
{
    private readonly SanctionPolicyAdapter _policy;

    public SanctionEngine(SanctionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SanctionResult> ExecuteAsync(SanctionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SanctionResult(true, "Executed");
    }
}
