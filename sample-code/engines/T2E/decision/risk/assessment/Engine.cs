namespace Whycespace.Engines.T2E.Decision.Risk.Assessment;

public class AssessmentEngine
{
    private readonly AssessmentPolicyAdapter _policy;

    public AssessmentEngine(AssessmentPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AssessmentResult> ExecuteAsync(AssessmentCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AssessmentResult(true, "Executed");
    }
}
