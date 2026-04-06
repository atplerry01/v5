namespace Whycespace.Engines.T2E.Business.Entitlement.Eligibility;

public class EligibilityEngine
{
    private readonly EligibilityPolicyAdapter _policy;

    public EligibilityEngine(EligibilityPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EligibilityResult> ExecuteAsync(EligibilityCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EligibilityResult(true, "Executed");
    }
}
