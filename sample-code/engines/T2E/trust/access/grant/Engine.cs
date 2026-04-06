namespace Whycespace.Engines.T2E.Trust.Access.Grant;

public class GrantEngine
{
    private readonly GrantPolicyAdapter _policy;

    public GrantEngine(GrantPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<GrantResult> ExecuteAsync(GrantCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new GrantResult(true, "Executed");
    }
}
