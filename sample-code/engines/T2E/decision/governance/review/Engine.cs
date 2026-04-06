namespace Whycespace.Engines.T2E.Decision.Governance.Review;

public class ReviewEngine
{
    private readonly ReviewPolicyAdapter _policy;

    public ReviewEngine(ReviewPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ReviewResult> ExecuteAsync(ReviewCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ReviewResult(true, "Executed");
    }
}
