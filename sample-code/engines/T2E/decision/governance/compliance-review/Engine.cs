namespace Whycespace.Engines.T2E.Decision.Governance.ComplianceReview;

public class ComplianceReviewEngine
{
    private readonly ComplianceReviewPolicyAdapter _policy;

    public ComplianceReviewEngine(ComplianceReviewPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ComplianceReviewResult> ExecuteAsync(ComplianceReviewCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ComplianceReviewResult(true, "Executed");
    }
}
