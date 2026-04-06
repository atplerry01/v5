namespace Whycespace.Engines.T2E.Business.Subscription.Renewal;

public class RenewalEngine
{
    private readonly RenewalPolicyAdapter _policy;

    public RenewalEngine(RenewalPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<RenewalResult> ExecuteAsync(RenewalCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new RenewalResult(true, "Executed");
    }
}
