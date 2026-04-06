namespace Whycespace.Engines.T2E.Constitutional.Policy.Version;

public class PolicyVersionEngine
{
    private readonly PolicyVersionPolicyAdapter _policy;

    public PolicyVersionEngine(PolicyVersionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PolicyVersionResult> ExecuteAsync(PolicyVersionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new PolicyVersionResult(true, "Executed");
    }
}
