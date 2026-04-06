namespace Whycespace.Engines.T2E.Business.Execution.Lifecycle;

public class LifecycleEngine
{
    private readonly LifecyclePolicyAdapter _policy;

    public LifecycleEngine(LifecyclePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<LifecycleResult> ExecuteAsync(LifecycleCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new LifecycleResult(true, "Executed");
    }
}
