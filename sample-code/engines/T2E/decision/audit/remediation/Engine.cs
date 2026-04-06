namespace Whycespace.Engines.T2E.Decision.Audit.Remediation;

public class RemediationEngine
{
    private readonly RemediationPolicyAdapter _policy;

    public RemediationEngine(RemediationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RemediationResult> ExecuteAsync(RemediationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RemediationResult(true, "Executed");
    }
}
