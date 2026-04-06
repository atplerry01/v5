namespace Whycespace.Engines.T2E.Business.Integration.Health;

public class HealthEngine
{
    private readonly HealthPolicyAdapter _policy;

    public HealthEngine(HealthPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<HealthResult> ExecuteAsync(HealthCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new HealthResult(true, "Executed");
    }
}
