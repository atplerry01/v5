namespace Whycespace.Engines.T2E.Business.Integration.Adapter;

public class AdapterEngine
{
    private readonly AdapterPolicyAdapter _policy;

    public AdapterEngine(AdapterPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AdapterResult> ExecuteAsync(AdapterCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AdapterResult(true, "Executed");
    }
}
