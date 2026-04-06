namespace Whycespace.Engines.T2E.Decision.Risk.IncidentRisk;

public class IncidentRiskEngine
{
    private readonly IncidentRiskPolicyAdapter _policy;

    public IncidentRiskEngine(IncidentRiskPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<IncidentRiskResult> ExecuteAsync(IncidentRiskCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new IncidentRiskResult(true, "Executed");
    }
}
