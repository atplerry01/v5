namespace Whycespace.Engines.T2E.Structural.Cluster.Continuity;

public class ContinuityEngine
{
    private readonly ContinuityPolicyAdapter _policy;
    public ContinuityEngine(ContinuityPolicyAdapter policy) { _policy = policy; }

    public async Task<ContinuityResult> ExecuteAsync(ContinuityCommand command)
    {
        await _policy.EnforceAsync(command);
        return new ContinuityResult(true, "Executed");
    }
}
