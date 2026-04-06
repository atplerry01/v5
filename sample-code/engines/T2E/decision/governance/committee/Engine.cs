namespace Whycespace.Engines.T2E.Decision.Governance.Committee;

public class CommitteeEngine
{
    private readonly CommitteePolicyAdapter _policy;

    public CommitteeEngine(CommitteePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<CommitteeResult> ExecuteAsync(CommitteeCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new CommitteeResult(true, "Executed");
    }
}
