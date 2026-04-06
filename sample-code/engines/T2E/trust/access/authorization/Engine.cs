namespace Whycespace.Engines.T2E.Trust.Access.Authorization;

public class AuthorizationEngine
{
    private readonly AuthorizationPolicyAdapter _policy;

    public AuthorizationEngine(AuthorizationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AuthorizationResult> ExecuteAsync(AuthorizationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Domain logic — emit events only (no persistence)

        return new AuthorizationResult(true, "Executed");
    }
}
