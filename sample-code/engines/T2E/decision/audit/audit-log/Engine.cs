namespace Whycespace.Engines.T2E.Decision.Audit.AuditLog;

public class AuditLogEngine
{
    private readonly AuditLogPolicyAdapter _policy;

    public AuditLogEngine(AuditLogPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AuditLogResult> ExecuteAsync(AuditLogCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AuditLogResult(true, "Executed");
    }
}
