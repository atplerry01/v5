namespace Whycespace.Engines.T2E.Trust.Access.Request;

public class AccessRequestEngine
{
    private readonly AccessRequestPolicyAdapter _policy;

    public AccessRequestEngine(AccessRequestPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AccessRequestResult> ExecuteAsync(AccessRequestCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new AccessRequestResult(true, "Executed");
    }
}
