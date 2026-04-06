namespace Whycespace.Engines.T2E.Business.Agreement.Acceptance;

public class AcceptanceEngine
{
    private readonly AcceptancePolicyAdapter _policy;

    public AcceptanceEngine(AcceptancePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AcceptanceResult> ExecuteAsync(AcceptanceCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AcceptanceResult(true, "Executed");
    }
}
