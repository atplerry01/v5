namespace Whycespace.Engines.T2E.Business.Resource.Utilization;

public class UtilizationEngine
{
    private readonly UtilizationPolicyAdapter _policy;

    public UtilizationEngine(UtilizationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<UtilizationResult> ExecuteAsync(UtilizationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new UtilizationResult(true, "Executed");
    }
}
