namespace Whycespace.Engines.T2E.Structural.Cluster.Lifecycle;

public class LifecycleEngine
{
    private readonly LifecyclePolicyAdapter _policy;
    public LifecycleEngine(LifecyclePolicyAdapter policy) { _policy = policy; }

    public async Task<LifecycleResult> ExecuteAsync(LifecycleCommand command)
    {
        await _policy.EnforceAsync(command);
        return new LifecycleResult(true, "Executed");
    }
}
