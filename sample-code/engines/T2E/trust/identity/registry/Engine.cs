namespace Whycespace.Engines.T2E.Trust.Identity.Registry;

public class IdentityRegistryEngine
{
    private readonly IdentityRegistryPolicyAdapter _policy;

    public IdentityRegistryEngine(IdentityRegistryPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<IdentityRegistryResult> ExecuteAsync(IdentityRegistryCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new IdentityRegistryResult(true, "Executed");
    }
}
