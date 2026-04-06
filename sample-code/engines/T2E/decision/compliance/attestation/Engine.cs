namespace Whycespace.Engines.T2E.Decision.Compliance.Attestation;

public class AttestationEngine
{
    private readonly AttestationPolicyAdapter _policy;

    public AttestationEngine(AttestationPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AttestationResult> ExecuteAsync(AttestationCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AttestationResult(true, "Executed");
    }
}
