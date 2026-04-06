namespace Whycespace.Engines.T2E.Decision.Compliance.Obligation;

public class ObligationEngine
{
    private readonly ObligationPolicyAdapter _policy;

    public ObligationEngine(ObligationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ObligationResult> ExecuteAsync(ObligationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ObligationResult(true, "Executed");
    }
}
