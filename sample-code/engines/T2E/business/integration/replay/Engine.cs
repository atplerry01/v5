namespace Whycespace.Engines.T2E.Business.Integration.Replay;

public class ReplayEngine
{
    private readonly ReplayPolicyAdapter _policy;

    public ReplayEngine(ReplayPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReplayResult> ExecuteAsync(ReplayCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReplayResult(true, "Executed");
    }
}
