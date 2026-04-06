namespace Whycespace.Engines.T2E.Business.Portfolio.Benchmark;

public class BenchmarkEngine
{
    private readonly BenchmarkPolicyAdapter _policy;

    public BenchmarkEngine(BenchmarkPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<BenchmarkResult> ExecuteAsync(BenchmarkCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new BenchmarkResult(true, "Executed");
    }
}
