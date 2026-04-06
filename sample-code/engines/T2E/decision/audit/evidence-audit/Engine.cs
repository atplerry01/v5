namespace Whycespace.Engines.T2E.Decision.Audit.EvidenceAudit;

public class EvidenceAuditEngine
{
    private readonly EvidenceAuditPolicyAdapter _policy;

    public EvidenceAuditEngine(EvidenceAuditPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<EvidenceAuditResult> ExecuteAsync(EvidenceAuditCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new EvidenceAuditResult(true, "Executed");
    }
}
