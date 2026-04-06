namespace Whycespace.Engines.T2E.Decision.Compliance.Regulation;

public class RegulationEngine
{
    private readonly RegulationPolicyAdapter _policy;

    public RegulationEngine(RegulationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RegulationResult> ExecuteAsync(RegulationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RegulationResult(true, "Executed");
    }
}
