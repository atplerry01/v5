namespace Whycespace.Engines.T2E.Business.Resource.Facility;

public class FacilityEngine
{
    private readonly FacilityPolicyAdapter _policy;

    public FacilityEngine(FacilityPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FacilityResult> ExecuteAsync(FacilityCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new FacilityResult(true, "Executed");
    }
}
