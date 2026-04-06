namespace Whycespace.Engines.T2E.Business.Integration.Credential;

public class CredentialEngine
{
    private readonly CredentialPolicyAdapter _policy;

    public CredentialEngine(CredentialPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CredentialResult> ExecuteAsync(CredentialCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CredentialResult(true, "Executed");
    }
}
