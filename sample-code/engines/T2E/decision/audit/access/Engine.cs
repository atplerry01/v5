namespace Whycespace.Engines.T2E.Decision.Audit.Access;

public class AccessEngine
{
    private readonly AccessPolicyAdapter _policy;

    public AccessEngine(AccessPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AccessResult> ExecuteAsync(AccessCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AccessResult(true, "Executed");
    }
}
