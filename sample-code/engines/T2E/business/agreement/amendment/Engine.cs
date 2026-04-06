namespace Whycespace.Engines.T2E.Business.Agreement.Amendment;

public class AmendmentEngine
{
    private readonly AmendmentPolicyAdapter _policy;

    public AmendmentEngine(AmendmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AmendmentResult> ExecuteAsync(AmendmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AmendmentResult(true, "Executed");
    }
}
