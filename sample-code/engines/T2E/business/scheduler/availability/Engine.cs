namespace Whycespace.Engines.T2E.Business.Scheduler.Availability;

public class AvailabilityEngine
{
    private readonly AvailabilityPolicyAdapter _policy;

    public AvailabilityEngine(AvailabilityPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<AvailabilityResult> ExecuteAsync(AvailabilityCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new AvailabilityResult(true, "Executed");
    }
}
