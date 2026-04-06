namespace Whycespace.Engines.T2E.Business.Integration.Provider;

public class ProviderEngine
{
    private readonly ProviderPolicyAdapter _policy;

    public ProviderEngine(ProviderPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ProviderResult> ExecuteAsync(ProviderCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ProviderResult(true, "Executed");
    }
}
