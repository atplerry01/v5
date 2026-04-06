namespace Whycespace.Engines.T2E.Business.Inventory.Count;

public class CountEngine
{
    private readonly CountPolicyAdapter _policy;

    public CountEngine(CountPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CountResult> ExecuteAsync(CountCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CountResult(true, "Executed");
    }
}
