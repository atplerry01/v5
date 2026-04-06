namespace Whycespace.Engines.T2E.Decision.Governance.Resolution;

public class ResolutionEngine
{
    private readonly ResolutionPolicyAdapter _policy;

    public ResolutionEngine(ResolutionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ResolutionResult> ExecuteAsync(ResolutionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ResolutionResult(true, "Executed");
    }
}
