namespace Whycespace.Engines.T2E.Decision.Compliance.Jurisdiction;

public class JurisdictionEngine
{
    private readonly JurisdictionPolicyAdapter _policy;

    public JurisdictionEngine(JurisdictionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<JurisdictionResult> ExecuteAsync(JurisdictionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new JurisdictionResult(true, "Executed");
    }
}
