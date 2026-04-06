namespace Whycespace.Engines.T2E.Business.Entitlement.EntitlementGrant;

public class EntitlementGrantEngine
{
    private readonly EntitlementGrantPolicyAdapter _policy;

    public EntitlementGrantEngine(EntitlementGrantPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EntitlementGrantResult> ExecuteAsync(EntitlementGrantCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EntitlementGrantResult(true, "Executed");
    }
}
