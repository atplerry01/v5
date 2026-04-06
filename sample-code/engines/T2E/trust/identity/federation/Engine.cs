namespace Whycespace.Engines.T2E.Trust.Identity.Federation;

public class FederationEngine
{
    private readonly FederationPolicyAdapter _policy;

    public FederationEngine(FederationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FederationResult> ExecuteAsync(FederationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new FederationResult(true, "Executed");
    }
}
