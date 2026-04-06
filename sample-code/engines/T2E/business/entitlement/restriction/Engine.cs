namespace Whycespace.Engines.T2E.Business.Entitlement.Restriction;

public class RestrictionEngine
{
    private readonly RestrictionPolicyAdapter _policy;

    public RestrictionEngine(RestrictionPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RestrictionResult> ExecuteAsync(RestrictionCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RestrictionResult(true, "Executed");
    }
}
