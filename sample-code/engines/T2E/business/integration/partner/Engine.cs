namespace Whycespace.Engines.T2E.Business.Integration.Partner;

public class PartnerEngine
{
    private readonly PartnerPolicyAdapter _policy;

    public PartnerEngine(PartnerPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<PartnerResult> ExecuteAsync(PartnerCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new PartnerResult(true, "Executed");
    }
}
