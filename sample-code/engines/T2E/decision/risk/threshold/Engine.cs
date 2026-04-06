namespace Whycespace.Engines.T2E.Decision.Risk.Threshold;

public class ThresholdEngine
{
    private readonly ThresholdPolicyAdapter _policy;

    public ThresholdEngine(ThresholdPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ThresholdResult> ExecuteAsync(ThresholdCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ThresholdResult(true, "Executed");
    }
}
