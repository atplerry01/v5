namespace Whycespace.Engines.T2E.Business.Integration.Registry;

public class RegistryEngine
{
    private readonly RegistryPolicyAdapter _policy;

    public RegistryEngine(RegistryPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RegistryResult> ExecuteAsync(RegistryCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RegistryResult(true, "Executed");
    }
}
