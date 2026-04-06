namespace Whycespace.Engines.T2E.Business.Agreement.Clause;

public class ClauseEngine
{
    private readonly ClausePolicyAdapter _policy;

    public ClauseEngine(ClausePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ClauseResult> ExecuteAsync(ClauseCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ClauseResult(true, "Executed");
    }
}
