namespace Whycespace.Engines.T2E.Business.Marketplace.Bid;

public class BidEngine
{
    private readonly BidPolicyAdapter _policy;

    public BidEngine(BidPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<BidResult> ExecuteAsync(BidCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new BidResult(true, "Executed");
    }
}
