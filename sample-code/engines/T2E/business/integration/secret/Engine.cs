namespace Whycespace.Engines.T2E.Business.Integration.Secret;

public class SecretEngine
{
    private readonly SecretPolicyAdapter _policy;

    public SecretEngine(SecretPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<SecretResult> ExecuteAsync(SecretCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new SecretResult(true, "Executed");
    }
}
