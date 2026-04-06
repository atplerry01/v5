namespace Whycespace.Engines.T2E.Decision.Risk.Mitigation;

public class MitigationEngine
{
    private readonly MitigationPolicyAdapter _policy;

    public MitigationEngine(MitigationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MitigationResult> ExecuteAsync(MitigationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MitigationResult(true, "Executed");
    }
}
