namespace Whycespace.Engines.T2E.Business.Logistic.Tracking;

public class TrackingEngine
{
    private readonly TrackingPolicyAdapter _policy;

    public TrackingEngine(TrackingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<TrackingResult> ExecuteAsync(TrackingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new TrackingResult(true, "Executed");
    }
}
