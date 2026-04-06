namespace Whycespace.Engines.T2E.Decision.Governance.GovernanceRecord;

public class GovernanceRecordEngine
{
    private readonly GovernanceRecordPolicyAdapter _policy;

    public GovernanceRecordEngine(GovernanceRecordPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<GovernanceRecordResult> ExecuteAsync(GovernanceRecordCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new GovernanceRecordResult(true, "Executed");
    }
}
