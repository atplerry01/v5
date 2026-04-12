namespace Whycespace.Domain.EconomicSystem.Revenue.Revenue;

public sealed class RevenueTraceService
{
    public bool ValidateOrigin(RevenueAggregate revenue) =>
        revenue.ContractId != Guid.Empty;
}
