namespace Whycespace.Engines.T2E.Business.Resource.MaintenanceResource;

public class MaintenanceResourceEngine
{
    private readonly MaintenanceResourcePolicyAdapter _policy;

    public MaintenanceResourceEngine(MaintenanceResourcePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MaintenanceResourceResult> ExecuteAsync(MaintenanceResourceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MaintenanceResourceResult(true, "Executed");
    }
}
