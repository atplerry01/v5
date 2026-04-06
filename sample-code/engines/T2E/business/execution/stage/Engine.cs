namespace Whycespace.Engines.T2E.Business.Execution.Stage;

public class StageEngine
{
    private readonly StagePolicyAdapter _policy;

    public StageEngine(StagePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<StageResult> ExecuteAsync(StageCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new StageResult(true, "Executed");
    }
}
