namespace Whycespace.Engines.T2E.Decision.Governance.Mandate;

public class MandateEngine
{
    private readonly MandatePolicyAdapter _policy;

    public MandateEngine(MandatePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MandateResult> ExecuteAsync(MandateCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MandateResult(true, "Executed");
    }
}
