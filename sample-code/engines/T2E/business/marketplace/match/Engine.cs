namespace Whycespace.Engines.T2E.Business.Marketplace.Match;

public class MatchEngine
{
    private readonly MatchPolicyAdapter _policy;

    public MatchEngine(MatchPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<MatchResult> ExecuteAsync(MatchCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new MatchResult(true, "Executed");
    }
}
