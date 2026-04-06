namespace Whycespace.Engines.T2E.Business.Execution.Milestone;

public class MilestoneEngine
{
    private readonly MilestonePolicyAdapter _policy;

    public MilestoneEngine(MilestonePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MilestoneResult> ExecuteAsync(MilestoneCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MilestoneResult(true, "Executed");
    }
}
