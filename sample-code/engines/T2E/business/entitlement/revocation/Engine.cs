namespace Whycespace.Engines.T2E.Business.Entitlement.Revocation;

public class RevocationEngine
{
    private readonly RevocationPolicyAdapter _policy;

    public RevocationEngine(RevocationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RevocationResult> ExecuteAsync(RevocationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RevocationResult(true, "Executed");
    }
}
