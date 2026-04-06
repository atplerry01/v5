namespace Whycespace.Engines.T2E.Decision.Governance.Authority;

public class AuthorityEngine
{
    private readonly AuthorityPolicyAdapter _policy;

    public AuthorityEngine(AuthorityPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AuthorityResult> ExecuteAsync(AuthorityCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AuthorityResult(true, "Executed");
    }
}
