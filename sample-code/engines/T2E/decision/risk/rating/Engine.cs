namespace Whycespace.Engines.T2E.Decision.Risk.Rating;

public class RatingEngine
{
    private readonly RatingPolicyAdapter _policy;

    public RatingEngine(RatingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RatingResult> ExecuteAsync(RatingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RatingResult(true, "Executed");
    }
}
