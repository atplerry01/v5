namespace Whycespace.Engines.T2E.Constitutional.Policy.Registry;

public class PolicyRegistryEngine
{
    private readonly PolicyRegistryPolicyAdapter _policy;

    public PolicyRegistryEngine(PolicyRegistryPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PolicyRegistryResult> ExecuteAsync(PolicyRegistryCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new PolicyRegistryResult(true, "Executed");
    }
}
