namespace Whycespace.Engines.T2E.Constitutional.Policy.Access;

public class PolicyAccessEngine
{
    private readonly PolicyAccessPolicyAdapter _policy;

    public PolicyAccessEngine(PolicyAccessPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PolicyAccessResult> ExecuteAsync(PolicyAccessCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new PolicyAccessResult(true, "Executed");
    }
}
