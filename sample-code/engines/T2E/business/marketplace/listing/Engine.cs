namespace Whycespace.Engines.T2E.Business.Marketplace.Listing;

public class ListingEngine
{
    private readonly ListingPolicyAdapter _policy;

    public ListingEngine(ListingPolicyAdapter policy)
    {
        _policy = policy;
    }

    public async Task<ListingResult> ExecuteAsync(ListingCommand command)
    {
        await _policy.EnforceAsync(command);

        // Emit domain event only — no persistence here

        return new ListingResult(true, "Executed");
    }
}
