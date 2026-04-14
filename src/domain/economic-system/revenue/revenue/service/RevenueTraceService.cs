namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueTraceService
{
    // Phase 2C: revenue origin is an SPV, not a contract. Origin validity
    // is "SpvId is non-empty"; structural existence is external responsibility.
    public bool ValidateOrigin(RevenueAggregate revenue) =>
        !string.IsNullOrWhiteSpace(revenue.SpvId);
}
