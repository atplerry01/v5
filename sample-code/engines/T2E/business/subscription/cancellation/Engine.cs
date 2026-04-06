namespace Whycespace.Engines.T2E.Business.Subscription.Cancellation;

public class CancellationEngine
{
    private readonly CancellationPolicyAdapter _policy;

    public CancellationEngine(CancellationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CancellationResult> ExecuteAsync(CancellationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CancellationResult(true, "Executed");
    }
}
