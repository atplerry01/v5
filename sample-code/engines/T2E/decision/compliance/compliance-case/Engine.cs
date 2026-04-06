namespace Whycespace.Engines.T2E.Decision.Compliance.ComplianceCase;

public class ComplianceCaseEngine
{
    private readonly ComplianceCasePolicyAdapter _policy;

    public ComplianceCaseEngine(ComplianceCasePolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ComplianceCaseResult> ExecuteAsync(ComplianceCaseCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ComplianceCaseResult(true, "Executed");
    }
}
