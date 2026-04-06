namespace Whycespace.Engines.T2E.Business.Marketplace.Offer;

public class OfferEngine
{
    private readonly OfferPolicyAdapter _policy;

    public OfferEngine(OfferPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<OfferResult> ExecuteAsync(OfferCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new OfferResult(true, "Executed");
    }
}
