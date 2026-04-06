namespace Whycespace.Engines.T2E.Business.Portfolio.Performance;

public class PerformanceEngine
{
    private readonly PerformancePolicyAdapter _policy;

    public PerformanceEngine(PerformancePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PerformanceResult> ExecuteAsync(PerformanceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PerformanceResult(true, "Executed");
    }
}
