namespace Whycespace.Engines.T2E.Decision.Governance.GovernanceCycle;

public class GovernanceCycleEngine
{
    private readonly GovernanceCyclePolicyAdapter _policy;

    public GovernanceCycleEngine(GovernanceCyclePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<GovernanceCycleResult> ExecuteAsync(GovernanceCycleCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new GovernanceCycleResult(true, "Executed");
    }
}
