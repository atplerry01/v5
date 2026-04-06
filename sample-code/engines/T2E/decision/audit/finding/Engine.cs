namespace Whycespace.Engines.T2E.Decision.Audit.Finding;

public class FindingEngine
{
    private readonly FindingPolicyAdapter _policy;

    public FindingEngine(FindingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<FindingResult> ExecuteAsync(FindingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new FindingResult(true, "Executed");
    }
}
