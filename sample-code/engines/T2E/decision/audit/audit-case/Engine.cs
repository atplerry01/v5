namespace Whycespace.Engines.T2E.Decision.Audit.AuditCase;

public class AuditCaseEngine
{
    private readonly AuditCasePolicyAdapter _policy;

    public AuditCaseEngine(AuditCasePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AuditCaseResult> ExecuteAsync(AuditCaseCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AuditCaseResult(true, "Executed");
    }
}
