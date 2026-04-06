namespace Whycespace.Engines.T2E.Business.Integration.Synchronization;

public class SynchronizationEngine
{
    private readonly SynchronizationPolicyAdapter _policy;

    public SynchronizationEngine(SynchronizationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SynchronizationResult> ExecuteAsync(SynchronizationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SynchronizationResult(true, "Executed");
    }
}
