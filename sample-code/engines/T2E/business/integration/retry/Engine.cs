namespace Whycespace.Engines.T2E.Business.Integration.Retry;

public class RetryEngine
{
    private readonly RetryPolicyAdapter _policy;

    public RetryEngine(RetryPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RetryResult> ExecuteAsync(RetryCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RetryResult(true, "Executed");
    }
}
