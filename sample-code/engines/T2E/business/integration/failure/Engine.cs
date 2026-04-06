namespace Whycespace.Engines.T2E.Business.Integration.Failure;

public class FailureEngine
{
    private readonly FailurePolicyAdapter _policy;

    public FailureEngine(FailurePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FailureResult> ExecuteAsync(FailureCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new FailureResult(true, "Executed");
    }
}
